using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Script.GameFramework.Data;
using Script.GameFramework.Infrastructure;
using Script.GameFramework.Manager;
using Script.Networks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

namespace Script.SceneManagers
{
    public class MenuManager : MonoBehaviour
    {
        private const string LobbySceneName = "Lobby";
        private string _hostIp;


        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        
        [SerializeField] private TMP_InputField lobbyCodeInputField;

        [SerializeField] private int maxPlayer = 4;
        [SerializeField] private Toggle inviteOnlyToggle;


        private async void Start()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await Authentication.Instance.SignInAnonymouslyAsync();
            }
            
            SceneTransitionHandler.Instance.SetSceneState(SceneTransitionHandler.SceneStates.MainMenu);
            
            hostButton.onClick.AddListener(StartLocalGame);
            joinButton.onClick.AddListener(JoinLocalGame);
        }

        private async void StartLocalGame()
        {
            LobbyPlayerData lobbyPlayerData = new();
            lobbyPlayerData.Initialize(AuthenticationService.Instance.PlayerId,  $"Player{Guid.NewGuid()}", "0", "HostPlayer");

            await LobbyManager.Instance.CreateLobbyAsync(inviteOnlyToggle.isOn, maxPlayer, lobbyPlayerData.Serialize());
            
            SceneTransitionHandler.Instance.SwitchScene(LobbySceneName);
        }

        private async void JoinLocalGame()
        {
            LobbyPlayerData lobbyPlayerData = new LobbyPlayerData();
            lobbyPlayerData.Initialize(AuthenticationService.Instance.PlayerId, $"Player{Guid.NewGuid()}", "1", "JoinPlayer");
            
            // Join a lobby
            bool lobbyJoined = await LobbyManager.Instance.JoinLobbyByCodeAsync(lobbyCodeInputField.text.ToUpper(), lobbyPlayerData.Serialize());
            
            if (!lobbyJoined)
            {
                Debug.LogError("Failed to join lobby.");
                return;
            }
            SceneTransitionHandler.Instance.SwitchScene(LobbySceneName);
        }
        
        private void StartHostNetwork()
        {
            
            var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            if (utpTransport) _hostIp = "127.0.0.1";
            if (NetworkManager.Singleton.StartHost())
            {
                SceneTransitionHandler.Instance.RegisterCallbacks();
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