using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Script.GameFramework.Core;
using Script.GameFramework.Data;
using Script.GameFramework.Infrastructure;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Script.GameFramework.Manager
{
    public class LobbyManager : Singleton<LobbyManager>
    {
        private Lobby _lobby;
        private Coroutine _heartbeatCoroutine;
        private Coroutine _refreshCoroutine;
        
        private const float HeartbeatInterval = 15f; // Rate limit is 5 request per 30 seconds
        private const float RefreshInterval = 2f; // Rate limit is 1 request per second

        public static event Action<LobbyCreatedEventArgs> OnLobbyCreated;
        public static event Action<LobbyJoinedEventArgs> OnLobbyJoined;
        public static event Action OnLobbyUpdated;

        public async Task<bool> JoinLobbyByCodeAsync(string code, Dictionary<string, string> playerData)
        {
            
            JoinLobbyByCodeOptions options = new()
            {
                Player = new Player(AuthenticationService.Instance.PlayerId, null, SerializePlayerData(playerData))
            };

            try
            {
                _lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code, options);
            }
            catch (Exception)
            {
                return false;
            }

            _refreshCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, RefreshInterval)); 
            
            OnLobbyJoined?.Invoke(new LobbyJoinedEventArgs
            {
                ClientId = options.Player.Id,
            });
            
            return true;
        }

        public async Task<bool> CreateLobbyAsync(bool isPrivate,int maxPlayer, Dictionary<string, string> data)
        {

            var playerData = SerializePlayerData(data);

            Player player = new(AuthenticationService.Instance.PlayerId, null,playerData);

            CreateLobbyOptions options = new()
            {
                IsPrivate = isPrivate,
                Player = player
            };

            try
            {
                _lobby = await Lobbies.Instance.CreateLobbyAsync("Lobby Name Test", maxPlayer, options);
            }
            catch (Exception)
            {
                return false;
            }

            _heartbeatCoroutine = StartCoroutine(HeartbeatLobbyCoroutine(_lobby.Id, HeartbeatInterval));
            _refreshCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, RefreshInterval));
            
            OnLobbyCreated?.Invoke(new LobbyCreatedEventArgs
            {
                HostId = _lobby.HostId,
                IsPrivate = _lobby.IsPrivate,
                LobbyCode = _lobby.LobbyCode,
                MaxPlayer = _lobby.MaxPlayers,
                LobbyId = _lobby.Id
            });
            
            Debug.Log($"Lobby created with id {_lobby.Id} and code {_lobby.LobbyCode}");
            
            return true;
        }
        
        public async Task UpdatePlayerDataAsync(string playerId, Dictionary<string, string> data)
        {
            var playerData = SerializePlayerData(data);
            await Lobbies.Instance.UpdatePlayerAsync(_lobby.Id, playerId, new UpdatePlayerOptions()
            {
                Data = playerData
            });
        }

        private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
        {
            WaitForSecondsRealtime delay = new(waitTimeSeconds);
            Debug.Log($"Starting Lobby {lobbyId} heartbeat...");
            
            while (true)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return delay;
            }
        }
        
        private IEnumerator RefreshLobbyCoroutine(string lobbyId, float waitTimeSeconds)
        {
            Stopwatch stopwatch = new();
            Debug.Log($"Staring Lobby {lobbyId} refresh...");
            
            while (true)
            {
                stopwatch.Restart();
                var task = LobbyService.Instance.GetLobbyAsync(lobbyId);
                yield return new WaitUntil(() => task.IsCompleted);
                Lobby newLobby = task.Result;
                if (newLobby.LastUpdated > _lobby.LastUpdated)
                {
                    _lobby = newLobby;
                    OnLobbyUpdated?.Invoke();
                }
                
                float delay = Mathf.Max(0, 1 - stopwatch.ElapsedMilliseconds * 1000);
                
                yield return new WaitForSeconds(delay);
            }
        }

        private async Task ManualRefreshLobby()
        {
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_lobby.Id);
            if (lobby.LastUpdated > _lobby.LastUpdated)
            {
                _lobby = lobby;
                OnLobbyUpdated?.Invoke();
                
                Debug.Log($"Lobby {_lobby.Id} refreshed");
            }
        }

        private static Dictionary<string, PlayerDataObject> SerializePlayerData(Dictionary<string, string> data)
        {
            return data.ToDictionary(item => item.Key,
                item => new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, item.Value));
        }
        
        public List<Dictionary<string,PlayerDataObject>> GetPlayerData()
        {
            return _lobby.Players.Select(player => player.Data).ToList();
        }
        
        public async void OnApplicationQuit()
        {
            Debug.Log("Quitting application...");


            if (_lobby == null) return;
            
            if (IsHost()) await DeleteLobbyAsync();
            else await LeaveLobby();
        }
        
        private async Task DeleteLobbyAsync()
        {
            if (_lobby == null) return;
            StopCoroutine(_heartbeatCoroutine);
            StopCoroutine(_refreshCoroutine);
            _heartbeatCoroutine = null;
            _refreshCoroutine = null;
            await LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
            _lobby = null;
        }

        private async Task LeaveLobby()
        {
            if (_lobby == null) return;
            await LobbyService.Instance.RemovePlayerAsync(_lobby.Id, AuthenticationService.Instance.PlayerId);
            StopCoroutine(_refreshCoroutine);
            _refreshCoroutine = null;
            _lobby = null;
        }
        
        public bool IsInLobby()
        {
            return _lobby != null;
        }
        
        public bool IsHost()
        {
            return _lobby != null && _lobby.HostId == AuthenticationService.Instance.PlayerId;
        }
        
        public bool IsPlayerInLobby(string playerId)
        {
            return _lobby != null && _lobby.Players.Any(player => player.Id == playerId);
        }
        
        public bool IsPlayerHost(string playerId)
        {
            return _lobby != null && _lobby.HostId == playerId;
        }
        
        public bool IsPlayerReady(string playerId)
        {
            return _lobby != null && GetPlayerData().Any(a => a["isReady"].Value == "true" && a["playerId"].Value == playerId);
        }
        
        public bool IsPlayerReady()
        {
            return _lobby != null && GetPlayerData().Any(a => a["isReady"].Value == true.ToString() && a["playerId"].Value == AuthenticationService.Instance.PlayerId);
        }
        
        public bool IsAllPlayerReady()
        {
            return _lobby != null && GetPlayerData().All(a => a["isReady"].Value == true.ToString());
        }

        public string GetMaxPlayer()
        {
            return _lobby.MaxPlayers.ToString();
        }
    }
}