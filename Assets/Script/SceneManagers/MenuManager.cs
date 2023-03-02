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

        private void Start()
        {
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
    }
}