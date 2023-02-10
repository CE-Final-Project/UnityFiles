using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Script.GameFramework.Core;
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
        private LobbyData _currentLobbyData;
        private Coroutine _heartbeatCoroutine;
        private Coroutine _refreshCoroutine;
        
        public LobbyData CurrentLobbyData => _currentLobbyData;

        public async Task<bool> JoinLobbyByCodeAsync(string code)
        {
            _lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code);
            
            if (_lobby == null)
            {
                return false;
            }
            
            _currentLobbyData = new LobbyData
            {
                LobbyId = _lobby.Id,
                LobbyName = _lobby.Name,
                LobbyCode = _lobby.LobbyCode,
                IsPrivate = _lobby.IsPrivate,
                HostId = _lobby.HostId,
                MaxPlayer = _lobby.MaxPlayers
            };
            
            _refreshCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f)); // Rate limit is 1 request per second
            
            return true;
        }

        public async Task<bool> CreateLobbyAsync(bool isPrivate,int maxPlayer, Dictionary<string, string> data)
        {

            var playerData = SerializePlayerData(data);

            var player = new Player(AuthenticationService.Instance.PlayerId, null,playerData);

            var options = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = player
            };

            try
            {
                _lobby = await Lobbies.Instance.CreateLobbyAsync("Lobby Name Test", maxPlayer, options);
                _currentLobbyData = new LobbyData
                {
                    LobbyId = _lobby.Id,
                    LobbyName = _lobby.Name,
                    LobbyCode = _lobby.LobbyCode,
                    IsPrivate = _lobby.IsPrivate,
                    HostId = _lobby.HostId,
                    MaxPlayer = _lobby.MaxPlayers
                };
            }
            catch (Exception)
            {
                return false;
            }

            Debug.Log($"Lobby {_lobby.Id} created");

            _heartbeatCoroutine = StartCoroutine(HeartbeatLobbyCoroutine(_lobby.Id, 6f)); // Rate limit is 5 request per 30 seconds
            _refreshCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f)); // Rate limit is 1 request per second
            
            return true;
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
                    _currentLobbyData = new LobbyData
                    {
                        LobbyId = newLobby.Id,
                        LobbyName = newLobby.Name,
                        LobbyCode = newLobby.LobbyCode,
                        IsPrivate = newLobby.IsPrivate,
                        HostId = newLobby.HostId,
                        MaxPlayer = newLobby.MaxPlayers
                    };
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
            
            Debug.Log("Signing out...");
            
            Authentication.Instance.SignOut();
        }
    }
}