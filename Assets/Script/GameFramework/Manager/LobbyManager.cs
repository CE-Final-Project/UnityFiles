using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Script.GameFramework.Core;
using Script.GameFramework.Data;
using Script.GameFramework.Infrastructure;
using Script.GameFramework.Models;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Script.GameFramework.Manager
{
    public class LobbyManager : Singleton<LobbyManager>
    {
        private Lobby _lobby;
        private Coroutine _heartbeatCoroutine;
        private Coroutine _refreshCoroutine;

        public static event Action<LobbyCreatedEventArgs> OnLobbyCreated;
        public static event Action<LobbyJoinedEventArgs> OnLobbyJoined;
        public static event Action<Lobby> OnLobbyUpdated;

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

            _refreshCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f)); // Rate limit is 1 request per second
            
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

            _heartbeatCoroutine = StartCoroutine(HeartbeatLobbyCoroutine(_lobby.Id, 6f)); // Rate limit is 5 request per 30 seconds
            _refreshCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f)); // Rate limit is 1 request per second
            
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
            while (true)
            {
                Debug.Log($"Lobby {lobbyId} heartbeat");
                LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return new WaitForSecondsRealtime(waitTimeSeconds);
            }
        }
        
        private IEnumerator RefreshLobbyCoroutine(string lobbyId, float waitTimeSeconds)
        {
            while (true)
            {
                var task = LobbyService.Instance.GetLobbyAsync(lobbyId);
                yield return new WaitUntil(() => task.IsCompleted);
                Lobby newLobby = task.Result;
                if (newLobby.LastUpdated > _lobby.LastUpdated)
                {
                    _lobby = newLobby;
                    OnLobbyUpdated?.Invoke(_lobby);
                    
                    Debug.Log($"Lobby {lobbyId} refreshed");
                }
                yield return new WaitForSecondsRealtime(waitTimeSeconds);
            }
        }

        private static Dictionary<string, PlayerDataObject> SerializePlayerData(Dictionary<string, string> data)
        {
            return data.ToDictionary(item => item.Key,
                item => new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, item.Value));
        }
        
        public async void OnApplicationQuit()
        {
            Debug.Log("Quitting application...");
            
            if (_lobby != null && _lobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                
                Debug.Log($"Lobby {_lobby.Id} deleted");
                
                await Lobbies.Instance.DeleteLobbyAsync(_lobby.Id);
            }
        }

        public List<Dictionary<string,PlayerDataObject>> GetPlayerData()
        {
            List<Dictionary<string, PlayerDataObject>> data = new();

            foreach (Player player in _lobby.Players)
            {
                data.Add(player.Data);
            }

            return data;
        }
    }
}