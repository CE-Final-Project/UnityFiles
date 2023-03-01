using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Script.SceneManagers
{
    public class GameLobbyManager : MonoBehaviour
    {
        
        [SerializeField] private Button lobbyButton;
        [SerializeField] private TextMeshProUGUI textButton;
        [SerializeField] private TextMeshProUGUI lobbyCodeText;
        [SerializeField] private TextMeshProUGUI lobbyCountPlayerText;

        private string _hostIp = "127.0.0.1";
        private readonly List<LobbyPlayerData> _listLobbyPlayerData = new(); 
        private LobbyPlayerData _localPlayerData;

        private void Start()
        {
            SceneTransitionHandler.Instance.SetSceneState(SceneTransitionHandler.SceneStates.Lobby);
            
            lobbyButton.onClick.AddListener(OnClickHandler);

            if (LobbyManager.Instance.IsHost())
            {
                textButton.text = "Waiting for players...";
                lobbyButton.interactable = false;
            }
            else
            {
                textButton.text = "Ready";
            }

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
                    _hostIp = IPManager.GetIP(ADDRESSFAM.IPv4);
                    Debug.Log("All player ready, start game.");
                    Debug.Log($"Host IP: {_hostIp}");
                    await LobbyManager.Instance.UpdateLobbyGameStartedAsync(_hostIp);

                    return;
                }
            }
            
            // Update player data
            _localPlayerData.IsReady = !_localPlayerData.IsReady;
            await LobbyManager.Instance.UpdatePlayerDataAsync(_localPlayerData.Id, _localPlayerData.Serialize());
            textButton.text = _localPlayerData.IsReady ? "Unready" : "Ready";
            
            if (_localPlayerData.IsReady && !LobbyManager.Instance.IsHost())
            {
                LobbyManager.OnGameStarted += OnGameStarted;
            }
        }

        private void OnGameStarted()
        {
            
            Debug.Log("Game Started");
            
            _hostIp = LobbyManager.Instance.GetHostIp();

            lobbyButton.interactable = false;
            
            // Start Client Network
            StartClientNetwork();
        }

        private void OnDestroy()
        {
            // LobbyManager.OnLobbyCreated -= OnLobbyCreated;
            // LobbyManager.OnLobbyJoined -= OnLobbyJoined;
            LobbyManager.OnLobbyUpdated -= OnLobbyUpdated;
            LobbyManager.OnGameStarted -= OnGameStarted;
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
                lobbyButton.interactable = LobbyManager.Instance.IsAllPlayerReady();
            }
            
            lobbyCountPlayerText.text = $"Players ({_listLobbyPlayerData.Count}/{LobbyManager.Instance.GetMaxPlayer()})";

            // print all player data
            foreach (LobbyPlayerData data in _listLobbyPlayerData)
            {
                Debug.Log($"PlayerId: {data.Id}, PlayerName: {data.Name}, CharacterId: {data.CharacterId}, IsReady: {data.IsReady}");
            }
        }

        private static void StartHostNetwork()
        { 
            if (NetworkManager.Singleton.StartHost()) 
            {
               // NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectGame;
               SceneTransitionHandler.Instance.RegisterCallbacks();
               SceneTransitionHandler.Instance.SwitchScene("InGame");
               // SceneManager.LoadScene("inGame");
            }
            else
            {
                Debug.LogError("Failed to start host.");
            }
        }

        private void StartClientNetwork()
        {
            UnityTransport utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            if (utpTransport)
            {
                utpTransport.SetConnectionData(Sanitize(_hostIp), 12121);
            }

            if (!NetworkManager.Singleton.StartClient())
            {
                Debug.LogError("Failed to start client.");
            }
        }
        
        private static string GetLocalIPAddress()
        {
            
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                {
                    return ip.ToString();
                }
            }

            throw new System.Exception("No network adapters with an IPv4 address in the system!");
        }
        
        private static string Sanitize(string dirtyString)
        {
            // sanitize the input for the ip address
            return Regex.Replace(dirtyString, "[^A-Za-z0-9.]", "");
        }
    }
}