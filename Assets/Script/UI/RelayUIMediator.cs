using System;
using Script.Configuration;
using Script.ConnectionManagement;
using Script.Infrastructure.PubSub;
using Script.Lobby;
using Script.NGO;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Script.UI
{
    public class RelayUIMediator : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI playerNameLabel;
        [SerializeField] private RelayJoiningUI relayJoiningUI;
        [SerializeField] private RelayHostingUI relayHostingUI;
        [SerializeField] private IPConnectionWindow ipConnectionWindow;
        [FormerlySerializedAs("signInSpinner")] [SerializeField] private GameObject loadingSpinner;
        
        private AuthenticationServiceFacade _authenticationServiceFacade;
        private LobbyServiceFacade _lobbyServiceFacade;
        private LocalLobbyUser _localUser;
        private LocalLobby _localLobby;
        private NameGenerationData _nameGenerationData;
        private ConnectionManager _connectionManager;
        private ISubscriber<ConnectStatus> _connectStatusSubscriber;
        
        private const string DefaultLobbyName = "no-name";
        
        
        [Inject]
        private void InjectDependenciesAndInitialize(
            AuthenticationServiceFacade authenticationServiceFacade,
            LobbyServiceFacade lobbyServiceFacade,
            LocalLobbyUser localUser,
            LocalLobby localLobby,
            NameGenerationData nameGenerationData,
            ISubscriber<ConnectStatus> connectStatusSub,
            ConnectionManager connectionManager
        )
        {
            _authenticationServiceFacade = authenticationServiceFacade;
            _nameGenerationData = nameGenerationData;
            _localUser = localUser;
            _lobbyServiceFacade = lobbyServiceFacade;
            _localLobby = localLobby;
            _connectionManager = connectionManager;
            _connectStatusSubscriber = connectStatusSub;
            RegenerateName();

            _connectStatusSubscriber.Subscribe(OnConnectStatus);
        }

        private void Awake()
        {
            Hide();
        }

        private void OnConnectStatus(ConnectStatus status)
        {
            if (status is ConnectStatus.GenericDisconnect or ConnectStatus.StartClientFailed)
            {
                UnblockUIAfterLoadingIsComplete();
            }
        }

        private void OnDestroy()
        {
            _connectStatusSubscriber?.Unsubscribe(OnConnectStatus);
        }

        //Lobby and Relay calls done from UI

        public async void CreateLobbyRequest(string lobbyName, bool isPrivate)
        {
            // before sending request to lobby service, populate an empty lobby name, if necessary
            if (string.IsNullOrEmpty(lobbyName))
            {
                lobbyName = DefaultLobbyName;
            }

            BlockUIWhileLoadingIsInProgress();

            bool playerIsAuthorized = await _authenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                UnblockUIAfterLoadingIsComplete();
                return;
            }

            var lobbyCreationAttempt = await _lobbyServiceFacade.TryCreateLobbyAsync(lobbyName, _connectionManager.MaxConnectedPlayers, isPrivate);

            if (lobbyCreationAttempt.Success)
            {
                _localUser.IsHost = true;
                _lobbyServiceFacade.SetRemoteLobby(lobbyCreationAttempt.Lobby);

                Debug.Log($"Created lobby with ID: {_localLobby.LobbyID} and code {_localLobby.LobbyCode}");
                _connectionManager.StartHostLobby(_localUser.DisplayName);
            }
            else
            {
                UnblockUIAfterLoadingIsComplete();
            }
        }
        
        public async void JoinLobbyWithCodeRequest(string lobbyCode)
        {
            BlockUIWhileLoadingIsInProgress();

            bool playerIsAuthorized = await _authenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                UnblockUIAfterLoadingIsComplete();
                return;
            }

            var result = await _lobbyServiceFacade.TryJoinLobbyAsync(null, lobbyCode);

            if (result.Success)
            {
                OnJoinedLobby(result.Lobby);
            }
            else
            {
                UnblockUIAfterLoadingIsComplete();
            }
        }

        public async void JoinLobbyRequest(LocalLobby lobby)
        {
            BlockUIWhileLoadingIsInProgress();

            bool playerIsAuthorized = await _authenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                UnblockUIAfterLoadingIsComplete();
                return;
            }

            var result = await _lobbyServiceFacade.TryJoinLobbyAsync(lobby.LobbyID, lobby.LobbyCode);

            if (result.Success)
            {
                OnJoinedLobby(result.Lobby);
            }
            else
            {
                UnblockUIAfterLoadingIsComplete();
            }
        }

        private void OnJoinedLobby(Unity.Services.Lobbies.Models.Lobby remoteLobby)
        {
            _lobbyServiceFacade.SetRemoteLobby(remoteLobby);

            Debug.Log($"Joined lobby with code: {_localLobby.LobbyCode}, Internal Relay Join Code{_localLobby.RelayJoinCode}");
            _connectionManager.StartClientLobby(_localUser.DisplayName);
        }

        //show/hide UI

        public void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            relayHostingUI.Hide();
            relayJoiningUI.Hide();
        }

        public void ToggleJoinLobbyUI()
        {
            relayJoiningUI.Show();
            relayHostingUI.Hide();
            // m_JoinToggleHighlight.SetToColor(1);
            // m_JoinToggleTabBlocker.SetToColor(1);
            // m_CreateToggleHighlight.SetToColor(0);
            // m_CreateToggleTabBlocker.SetToColor(0);
        }

        public void ToggleCreateLobbyUI()
        {
            relayJoiningUI.Hide();
            relayHostingUI.Show();
            // m_JoinToggleHighlight.SetToColor(0);
            // m_JoinToggleTabBlocker.SetToColor(0);
            // m_CreateToggleHighlight.SetToColor(1);
            // m_CreateToggleTabBlocker.SetToColor(1);
        }

        public void RegenerateName()
        {
            _localUser.DisplayName = _nameGenerationData.GenerateName();
            playerNameLabel.text = _localUser.DisplayName;
        }

        private void BlockUIWhileLoadingIsInProgress()
        {
            canvasGroup.interactable = false;
            loadingSpinner.SetActive(true);
        }

        private void UnblockUIAfterLoadingIsComplete()
        {
            //this callback can happen after we've already switched to a different scene
            //in that case the canvas group would be null
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                loadingSpinner.SetActive(false);
            }
        }
        
    }
}