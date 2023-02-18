using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Script.GameFramework.Data;
using Script.GameFramework.Manager;
using Script.Networks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Script.SceneManagers
{
    public class GameLobbyManager : MonoBehaviour
    {
        
        [SerializeField] private Button lobbyButton;
        [SerializeField] private TextMeshProUGUI textButton;
        [SerializeField] private TextMeshProUGUI lobbyCodeText;
        [SerializeField] private TextMeshProUGUI lobbyCountPlayerText;

        private string _hostIp;
        private readonly List<LobbyPlayerData> _listLobbyPlayerData = new(); 
        private LobbyPlayerData _localPlayerData;

        private void Start()
        {
            SceneTransitionHandler.Instance.SetSceneState(SceneTransitionHandler.SceneStates.Lobby);
            
            lobbyButton.onClick.AddListener(OnClickHandler);
            textButton.text = LobbyManager.Instance.IsHost() ? "Start Game" : "Ready";

            LobbyManager.OnLobbyUpdated += OnLobbyUpdated;
        }

        private async void OnClickHandler()
        {
            if (LobbyManager.Instance.IsHost())
            {
                if (LobbyManager.Instance.IsAllPlayerReady())
                {
                    StartHostNetwork();
                    
                    // Update Lobby Status
                    await LobbyManager.Instance.UpdateLobbyGameStartedAsync();
                    
                    return;
                }
            }
            
            // Update player data
            _localPlayerData.IsReady = !_localPlayerData.IsReady;
            await LobbyManager.Instance.UpdatePlayerDataAsync(_localPlayerData.Id, _localPlayerData.Serialize());
            textButton.text = _localPlayerData.IsReady ? "Ready" : "Unready";
            
            if (_localPlayerData.IsReady && !LobbyManager.Instance.IsHost())
            {
                LobbyManager.OnGameStarted += OnGameStarted;
            }
        }

        private void OnGameStarted()
        {
            SceneTransitionHandler.Instance.RegisterCallbacks();
            SceneTransitionHandler.Instance.SwitchScene("InGame");
        }

        private void OnDestroy()
        {
            // LobbyManager.OnLobbyCreated -= OnLobbyCreated;
            // LobbyManager.OnLobbyJoined -= OnLobbyJoined;
            LobbyManager.OnLobbyUpdated -= OnLobbyUpdated;
        }

        private void OnLobbyUpdated()
        {
            var playerData = LobbyManager.Instance.GetPlayerData();
            _listLobbyPlayerData.Clear();
            foreach (var data in playerData)
            {
               LobbyPlayerData lobbyPlayerData = new();
               lobbyPlayerData.Initialize(data);
               
               if (lobbyPlayerData.Id == AuthenticationService.Instance.PlayerId)
               {
                   _localPlayerData = lobbyPlayerData;
               }
                  
               _listLobbyPlayerData.Add(lobbyPlayerData);
            }

            if (LobbyManager.Instance.IsHost() && _localPlayerData.IsReady)
            {
                textButton.text = LobbyManager.Instance.IsAllPlayerReady() ? "Start Game" : "Waiting for players...";
            }
            
            lobbyCountPlayerText.text = $"Players ({_listLobbyPlayerData.Count}/{LobbyManager.Instance.GetMaxPlayer()})";

            // print all player data
            foreach (LobbyPlayerData data in _listLobbyPlayerData)
            {
                Debug.Log($"PlayerId: {data.Id}, PlayerName: {data.Name}, CharacterId: {data.CharacterId}, IsReady: {data.IsReady}");
            }
        }
        

        private async void ReadyHandler()
        {
            _localPlayerData.IsReady = !_localPlayerData.IsReady;
            await LobbyManager.Instance.UpdatePlayerDataAsync(_localPlayerData.Id, _localPlayerData.Serialize());

            if (_localPlayerData.IsReady)
            {
                textButton.text = LobbyManager.Instance.IsHost() ? "Waiting for players..." : "Unready";
                if (!LobbyManager.Instance.IsHost())
                {
                    LobbyManager.OnGameStarted += HandleGameStarted;
                }
            }
            else
            {
                textButton.text = "Ready";
                if (!LobbyManager.Instance.IsHost())
                {
                    LobbyManager.OnGameStarted -= HandleGameStarted;
                }
            }
        }

        private void HandleGameStarted()
        {
            StartClientNetwork();
        }

        private async void StartLocalGame()
        {
            if (_listLobbyPlayerData.All(a => a.IsReady))
            {
                // host start game
                StartHostNetwork();

                await LobbyManager.Instance.UpdateLobbyGameStartedAsync();
            }
        }
        
        private void StartHostNetwork()
        { 
            UnityTransport utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
           if (utpTransport) _hostIp = "127.0.0.1";
           if (NetworkManager.Singleton.StartHost())
           {
               // NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectGame;
               SceneTransitionHandler.Instance.RegisterCallbacks();
               SceneTransitionHandler.Instance.SwitchScene("InGame");
           }
           else
           {
               Debug.LogError("Failed to start host.");
           }
        }

        private void OnClientConnectGame(ulong obj)
        {
            if (NetworkManager.Singleton.ConnectedClients.Count == LobbyManager.Instance.GetPlayerData().Count)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectGame;
            }
        }

        private void StartClientNetwork()
        {
            if (_hostIp == "Hostname") return;
            
            var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            if (utpTransport)
            {
                utpTransport.SetConnectionData(Sanitize(_hostIp), 7777);
            }
            
            if (!NetworkManager.Singleton.StartClient())
            {
                // SceneTransitionHandler.Instance.SwitchScene("InGame");
                Debug.LogError("Failed to start client.");
            }
        }
        
        private static string Sanitize(string dirtyString)
        {
            // sanitize the input for the ip address
            return Regex.Replace(dirtyString, "[^A-Za-z0-9.]", "");
        }
    }
}