using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Script.GameFramework.Data;
using Script.GameFramework.Manager;
using Script.GameFramework.Models;
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

        private string _hostIp;
        private List<LobbyPlayerData> _listLobbyPlayerData = new(); 
        private LobbyPlayerData _localPlayerData;

        private void Start()
        {
            SceneTransitionHandler.Instance.SetSceneState(SceneTransitionHandler.SceneStates.Lobby);
            
            LobbyManager.OnLobbyUpdated += OnLobbyUpdated;
        }
        
        private void OnDestroy()
        {
            // LobbyManager.OnLobbyCreated -= OnLobbyCreated;
            // LobbyManager.OnLobbyJoined -= OnLobbyJoined;
        }

        private void OnLobbyJoined(LobbyJoinedEventArgs args)
        {
            lobbyButton.onClick.AddListener(ReadyHandler);
            textButton.text = "Ready";
        }


        private void OnLobbyCreated(LobbyCreatedEventArgs args)
        { 
            lobbyButton.onClick.AddListener(StartLocalGame);
            textButton.text = "Start Game";
        }

        private void OnLobbyUpdated(Lobby lobby)
        {
            List<Dictionary<string, PlayerDataObject>> playerData = LobbyManager.Instance.GetPlayerData();
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
            
            if (AuthenticationService.Instance.PlayerId == lobby.HostId && _listLobbyPlayerData.All(a => a.IsReady))
            {
                lobbyButton.onClick.RemoveAllListeners();
                lobbyButton.onClick.AddListener(StartLocalGame);
                textButton.text = "Start Game";
            }
            else
            {
                lobbyButton.onClick.RemoveAllListeners();
                lobbyButton.onClick.AddListener(ReadyHandler);
                textButton.text = _localPlayerData.IsReady ? "Cancel" : "Ready";
            }
            
            Debug.Log($"Update: {_listLobbyPlayerData.Count}");
        }
        

        private async void ReadyHandler()
        {
            _localPlayerData.IsReady = !_localPlayerData.IsReady;
            await LobbyManager.Instance.UpdatePlayerDataAsync(_localPlayerData.Id, _localPlayerData.Serialize());
        }

        private void StartLocalGame()
        {
            if (_listLobbyPlayerData.All(a => a.IsReady))
            {
                SceneTransitionHandler.Instance.SwitchScene("InGame");
            }
        }
        
        private void StartHostNetwork()
        {
            
           var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
           if (utpTransport) _hostIp = "127.0.0.1";
           if (NetworkManager.Singleton.StartHost())
           {
               SceneTransitionHandler.Instance.RegisterCallbacks();
               SceneTransitionHandler.Instance.SwitchScene("InGame");
               NetworkManager.Singleton.OnServerStarted += () =>
               {
                   Debug.Log($"Server Started at {NetworkManager.Singleton.ConnectedHostname}");
               };
           }
           else
           {
               Debug.LogError("Failed to start host.");
           }
        }

        private void StartClientNetwork()
        {
            if (_hostIp == "Hostname") return;
            
            var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            if (utpTransport)
            {
                utpTransport.SetConnectionData(Sanitize(_hostIp), 7777);
                utpTransport.SetConnectionData("127.0.0.1", 7777);
            }
            
            if (!NetworkManager.Singleton.StartClient())
            {
                SceneTransitionHandler.Instance.SwitchScene("InGame");
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