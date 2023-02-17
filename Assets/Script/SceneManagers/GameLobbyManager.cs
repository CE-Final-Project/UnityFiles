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
            
            lobbyButton.onClick.AddListener(ReadyHandler);
            textButton.text = "Ready";

            LobbyManager.OnLobbyUpdated += OnLobbyUpdated;
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
                if (LobbyManager.Instance.IsAllPlayerReady())
                {
                    lobbyButton.onClick.RemoveListener(ReadyHandler);
                    lobbyButton.onClick.AddListener(StartLocalGame);
                    textButton.text = "Start Game";
                }
                else
                {
                    lobbyButton.onClick.RemoveListener(StartLocalGame);
                    lobbyButton.onClick.AddListener(ReadyHandler);
                    textButton.text = "Waiting for players...";
                }
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

            if (LobbyManager.Instance.IsHost())
            {
                if (LobbyManager.Instance.IsAllPlayerReady())
                {
                    lobbyButton.onClick.RemoveListener(ReadyHandler);
                    lobbyButton.onClick.AddListener(StartLocalGame);
                    textButton.text = "Start Game";
                }
                textButton.text = "Waiting for players...";
            }
            
            if (_localPlayerData.IsReady)
            {
                textButton.text = LobbyManager.Instance.IsHost() ? "Waiting for players..." : "Unready";
            }
            else
            {
                textButton.text = "Ready";
            }
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