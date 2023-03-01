using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Script.GameFramework.Core;
using Script.GameFramework.Data;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Script.GameFramework.Manager
{
    public class LobbyManager : Singleton<LobbyManager>
    {
        private Lobby _joinLobby;
        private Lobby _hostLobby;
        
        private float _heartbeatTimer;
        private  float _lobbyUpdateTimer;
        
        private const float HeartbeatInterval = 15f; // Rate limit is 5 request per 30 seconds
        private const float LobbyUpdateInterval = 1.5f; // Rate limit is 1 request per second
        
        public static event Action<LobbyCreatedEventArgs> OnLobbyCreated;
        public static event Action<LobbyJoinedEventArgs> OnLobbyJoined;
        public static event Action OnLobbyUpdated;
        public static event Action OnGameStarted;

        private void Update()
        {
            if (_joinLobby != null)
            {
                LobbyData lobbyData = new();
                lobbyData.Initialize(_joinLobby.Data);

                if (!lobbyData.IsGameStarted)
                {
                    HandleLobbyPollingForUpdates();
                }
            }
            HandleLobbyHeartbeat();
        }

        public async Task<bool> JoinLobbyByCodeAsync(string code, Dictionary<string, string> playerData)
        {
            
            JoinLobbyByCodeOptions options = new()
            {
                Player = new Player(AuthenticationService.Instance.PlayerId, null, SerializePlayerData(playerData))
            };

            try
            {
                _joinLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code, options);
            }
            catch (Exception)
            {
                return false;
            }

            OnLobbyJoined?.Invoke(new LobbyJoinedEventArgs
            {
                ClientId = options.Player.Id,
            });
            
            return true;
        }

        public async Task<bool> CreateLobbyAsync(bool isPrivate,int maxPlayer, Dictionary<string, string> data)
        {

            var playerData = SerializePlayerData(data);
            playerData["isReady"].Value = true.ToString();

            Player player = new(AuthenticationService.Instance.PlayerId, null,playerData);

            CreateLobbyOptions options = new()
            {
                IsPrivate = isPrivate,
                Player = player,
            };

            LobbyData lobbyData = new();
            lobbyData.Initialize(options.Data);
            
            options.Data = lobbyData.Serialize();

            try
            {
                _hostLobby = await Lobbies.Instance.CreateLobbyAsync("Lobby Name Test", maxPlayer, options);
                _joinLobby = _hostLobby;
            }
            catch (Exception)
            {
                return false;
            }

            OnLobbyCreated?.Invoke(new LobbyCreatedEventArgs
            {
                HostId = _hostLobby.HostId,
                IsPrivate = _hostLobby.IsPrivate,
                LobbyCode = _hostLobby.LobbyCode,
                MaxPlayer = _hostLobby.MaxPlayers,
                LobbyId = _hostLobby.Id
            });
            
            Debug.Log($"Lobby created with id {_hostLobby.Id} and code {_hostLobby.LobbyCode}");

            return true;
        }

        public async Task UpdatePlayerDataAsync(string playerId, Dictionary<string, string> data)
        {
            var playerData = SerializePlayerData(data);
            await Lobbies.Instance.UpdatePlayerAsync(_joinLobby.Id, playerId, new UpdatePlayerOptions()
            {
                Data = playerData
            });
        }

        private async void HandleLobbyHeartbeat()
        {
            if (_hostLobby == null) return;
            
            _heartbeatTimer -= Time.deltaTime;

            if (_heartbeatTimer < 0f)
            {
                _heartbeatTimer = HeartbeatInterval;

                await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
            }
        }

        private async void HandleLobbyPollingForUpdates()
        {
            if (_joinLobby == null) return;
            
            _lobbyUpdateTimer -= Time.deltaTime;

            if (_lobbyUpdateTimer < 0f)
            {
                _lobbyUpdateTimer = LobbyUpdateInterval;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_joinLobby.Id);
                _joinLobby = lobby;
                
                LobbyData lobbyData = new();
                lobbyData.Initialize(_joinLobby.Data);
                
                if (lobbyData.IsGameStarted)
                {
                    OnGameStarted?.Invoke();
                }
                
                OnLobbyUpdated?.Invoke();
            }
        }

        private async Task ManualRefreshLobby()
        {
            if (_joinLobby == null) return;

            if (_lobbyUpdateTimer <= 0f)
            {
                _lobbyUpdateTimer = LobbyUpdateInterval;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_joinLobby.Id);
                _joinLobby = lobby;
                
                OnLobbyUpdated?.Invoke();
            }
        }
        
        public async Task UpdateLobbyGameStartedAsync()
        {
            if (_hostLobby == null) return;
            
            LobbyData lobbyData = new()
            {
                IsGameStarted = true
            };

            await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions()
            {
                Data = lobbyData.Serialize()
            });
        }

        private static Dictionary<string, PlayerDataObject> SerializePlayerData(Dictionary<string, string> data)
        {
            return data.ToDictionary(item => item.Key,
                item => new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, item.Value));
        }
        
        public List<Dictionary<string,PlayerDataObject>> GetPlayerData()
        {
            return _joinLobby.Players.Select(player => player.Data).ToList();
        }
        
        public async void OnApplicationQuit()
        {
            Debug.Log("Quitting application...");

            if (_hostLobby == null)
            {
                await LeaveLobby();
            }
            else
            {
                await DeleteLobbyAsync();
            }
        }
        
        private async Task DeleteLobbyAsync()
        {
            if (_hostLobby == null) return;
            await LobbyService.Instance.DeleteLobbyAsync(_hostLobby.Id);
            _hostLobby = null;
        }

        private async Task LeaveLobby()
        {
            if (_joinLobby == null) return;
            await LobbyService.Instance.RemovePlayerAsync(_joinLobby.Id, AuthenticationService.Instance.PlayerId);
            _joinLobby = null;
        }
        
        private async Task KickPlayer(string playerId)
        {
            if (_hostLobby == null) return;
            await LobbyService.Instance.RemovePlayerAsync(_hostLobby.Id, playerId);
        }

        public bool IsInLobby()
        {
            return _hostLobby != null;
        }
        
        public bool IsHost()
        {
            return _hostLobby != null && _hostLobby.HostId == AuthenticationService.Instance.PlayerId;
        }
        
        public bool IsPlayerInLobby(string playerId)
        {
            return _hostLobby != null && _hostLobby.Players.Any(player => player.Id == playerId);
        }
        
        public bool IsPlayerHost(string playerId)
        {
            return _hostLobby != null && _hostLobby.HostId == playerId;
        }
        
        public bool IsPlayerReady(string playerId)
        {
            return _hostLobby != null && GetPlayerData().Any(a => a["isReady"].Value == "true" && a["playerId"].Value == playerId);
        }
        
        public bool IsPlayerReady()
        {
            return _hostLobby != null && GetPlayerData().Any(a => a["isReady"].Value == true.ToString() && a["playerId"].Value == AuthenticationService.Instance.PlayerId);
        }
        
        public bool IsAllPlayerReady()
        {
            return _hostLobby != null && GetPlayerData().All(a => a["isReady"].Value == true.ToString());
        }

        public string GetMaxPlayer()
        {
            return _joinLobby.MaxPlayers.ToString();
        }

        public string GetLobbyCode()
        {
            return _hostLobby.LobbyCode.ToString();
        }
    }
}