using System;
using Script.ApplicationLifecycle.Messages;
using Script.Configuration;
using Script.ConnectionManagement;
using Script.Infrastructure.PubSub;
using Script.Lobby;
using Script.NGO;
using Script.UI;
using Script.Utils;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Script.GameState
{
    public class ClientMainManuState : GameStateBehaviour
    {
        protected override GameState ActiveState => GameState.MainMenu;
        
        [SerializeField] private NameGenerationData nameGenerationData;
        [SerializeField] private RelayUIMediator relayUIMediator;
        [SerializeField] private IPUIMediator ipUiMediator;
        [SerializeField] private Button lobbyButton;

        [Inject] private AuthenticationServiceFacade _authenticationFacade;
        [Inject] private LocalLobbyUser _localUser;
        [Inject] private LocalLobby _localLobby;
        [Inject] private ProfileManager _profileManager;
        
        [Inject] private IPublisher<QuitApplicationMessage> _quitApplicationPub;

        protected override void Awake()
        {
            base.Awake();
            
            lobbyButton.interactable = false;
            
            if (string.IsNullOrEmpty(Application.cloudProjectId))
            {
                OnSignInFailed();
                return;
            }
            
            TrySignIn();
            
            AudioManager.Instance.StartMainMenuMusic();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterComponent(nameGenerationData);
            builder.RegisterComponent(relayUIMediator);
            builder.RegisterComponent(ipUiMediator);
        }
        
        private async void TrySignIn()
        {
            try
            {
                InitializationOptions unityAuthenticationInitOptions = new InitializationOptions();
                string profile = _profileManager.Profile;
                if (profile.Length > 0)
                {
                    unityAuthenticationInitOptions.SetProfile(profile);
                }

                await _authenticationFacade.InitializeAndSignInAsync(unityAuthenticationInitOptions);
                OnAuthSignIn();
                _profileManager.OnProfileChanged += OnProfileChanged;
            }
            catch (Exception)
            {
                OnSignInFailed();
            }
        }

        private void OnAuthSignIn()
        {
            lobbyButton.interactable = true;
            relayUIMediator.Hide();
            
            Debug.Log($"Signed in. Unity User ID: {AuthenticationService.Instance.PlayerId}");
            
            _localUser.ID = AuthenticationService.Instance.PlayerId;
            _localLobby.AddUser(_localUser);
        }
        
        private void OnSignInFailed()
        {
            if (lobbyButton)
            {
                lobbyButton.interactable = false;
            }
        }
        
        protected override void OnDestroy()
        {
            _profileManager.OnProfileChanged -= OnProfileChanged;
            base.OnDestroy();
        }
        
        private async void OnProfileChanged()
        {
            lobbyButton.interactable = false;
            await _authenticationFacade.SwitchProfileAndReSignInAsync(_profileManager.Profile);
            
            lobbyButton.interactable = true;
            
            Debug.Log($"Signed in. Unity User ID: {AuthenticationService.Instance.PlayerId}");
            
            // Update LocalUser and LocalLobby
            _localLobby.RemoveUser(_localUser);
            _localUser.ID = AuthenticationService.Instance.PlayerId;
            _localLobby.AddUser(_localUser);
        }

        public void OnRelayClicked()
        {
            relayUIMediator.ToggleJoinLobbyUI();
            relayUIMediator.Show();
        }
        
        public void OnDirectIPClicked()
        {
            relayUIMediator.Hide();
            ipUiMediator.Show();
        }
        
        public void OnChangeProfileClicked()
        {
            // uiProfileSelector.Show();
        }
        
        public void OnQuitClicked()
        {
            _quitApplicationPub.Publish(new QuitApplicationMessage());
        }
    }
}