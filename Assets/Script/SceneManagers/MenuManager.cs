using System;
#if UNITY_EDITOR
using ParrelSync;
#endif
using Script.GameFramework.Data;
using Script.GameFramework.Manager;
using Script.Networks;
using TMPro;
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

        private void Awake()
        {
            if (string.IsNullOrEmpty(Application.cloudProjectId))
            {
                OnSignInFailed();
                return;
            }
            TrySignIn();
        }
        
        private async void TrySignIn()
        {
            try
            {
                string serviceProfileName = "player";
#if UNITY_EDITOR
                serviceProfileName = $"{serviceProfileName}_{ClonesManager.GetCurrentProject().name}";
#endif
                await Auth.Auth.Authenticate(serviceProfileName, 2);
                OnAuthSignIn();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                OnSignInFailed();
            }
        }
        
        private void OnAuthSignIn()
        {
            Debug.Log($"Signed in. User Id: {AuthenticationService.Instance.PlayerId}");
        }
        
        private void OnSignInFailed()
        {
            Debug.LogError("Failed to sign in.");
        }

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