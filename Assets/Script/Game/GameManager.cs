using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ParrelSync;
using Script.Auth;
using Script.Lobby;
using Script.NGO;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Script.Game
{
    [Flags]
    public enum GameState
    {
        Menu = 1,
        Lobby = 2,
        JoinMenu = 4,
    }
    
    /// <summary>
    /// Sets up and runs the entire sample.
    /// All the Data that is important gets updated in here, the GameManager in the mainScene has all the references
    /// needed to run the game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public LocalLobby LocalLobby => _localLobby;
        
        public Action<GameState> OnGameStateChanged;
        public LocalLobbyList LocalLobbyList { get; private set; } = new LocalLobbyList();
        
        public GameState LocalGameState { get; private set; }
        public LobbyManager LobbyManager { get; private set; }
        
        [SerializeField] private CountDown countDown;

        private LocalPlayer _localUser;
        private LocalLobby _localLobby;
        private SetupInGame _setupInGame;
        
        private static GameManager _gameManagerInstance;

        public static GameManager Instance
        {
            get
            {
                if (_gameManagerInstance != null)
                    return _gameManagerInstance;
                _gameManagerInstance = FindObjectOfType<GameManager>();
                return _gameManagerInstance;
            }
        }
        
        public async Task<LocalPlayer> LocalUserInitializationAsync()
        {
            while (_localUser == null)
            {
                await Task.Delay(100);
            }
            return _localUser;
        }

        public async void CreateLobby(string lobbyName, bool isPrivate, int maxPlayers = 4)
        {
            try
            {
                Unity.Services.Lobbies.Models.Lobby lobby = await LobbyManager.CreateLobbyAsync(
                    lobbyName,
                    maxPlayers,
                    isPrivate, _localUser);

                LobbyConverters.RemoteToLocal(lobby, _localLobby);
                await CreateLobby();
            }
            catch (Exception exception)
            {
                // SetGameState(GameState.JoinMenu);
                Debug.LogError($"Error creating lobby : {exception} ");
            }
        }

        public async void JoinLobby(string lobbyId, string lobbyCode)
        {
            try
            {
                Unity.Services.Lobbies.Models.Lobby lobby = await LobbyManager.JoinLobbyAsync(lobbyId, lobbyCode, _localUser);
                LobbyConverters.RemoteToLocal(lobby, _localLobby);
                await JoinLobby();
            }
            catch (Exception e)
            {
                SetGameState(GameState.JoinMenu);
                Debug.LogError($"Error joining lobby : {e} ");
            }
        }

        public async void QueryLobbies()
        {
            LocalLobbyList.QueryState.Value = LobbyQueryState.Fetching;
            QueryResponse qr = await LobbyManager.RetrieveLobbyListAsync();
            if (qr == null)
            {
                return;
            }
            
            SetCurrentLobbies(LobbyConverters.QueryToLocalLobbyList(qr));
        }

        public void SetLocalUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                Debug.Log("Username is empty");
                return;
            }
            
            _localUser.DisplayName.Value = userName;
            SendLocalUserData();
        }
        
        public void SetLocalUserCharacter(CharacterType character)
        {
            _localUser.Character.Value = character;
            SendLocalUserData();
        }
        
        public void SetLocalUserStatus(PlayerStatus status)
        {
            _localUser.UserStatus.Value = status;
            SendLocalUserData();
        }
        
        private async void SendLocalLobbyData()
        {
            await LobbyManager.UpdateLobbyDataAsync(LobbyConverters.LocalToRemoteLobbyData(_localLobby));
        }
        
        private async void SendLocalUserData()
        {
            await LobbyManager.UpdatePlayerDataAsync(LobbyConverters.LocalToRemoteUserData(_localUser));
        }
        
        public void UIChangeMenuState(GameState state)
        {
            bool isQuittingGame = LocalGameState == GameState.Lobby &&
                                  _localLobby.LocalLobbyState.Value == LobbyState.InGame;

            if (isQuittingGame)
            {
                //If we were in-game, make sure we stop by the lobby first
                state = GameState.Lobby;
                ClientQuitGame();
            }
            SetGameState(state);
        }

        public void HostSetRelayCode(string code)
        {
            _localLobby.RelayCode.Value = code;
            SendLocalLobbyData();
        }

        //Only Host needs to listen to this and change state.
        private void OnPlayersReady(int readyCount)
        {
            if (readyCount == _localLobby.PlayerCount &&
                _localLobby.LocalLobbyState.Value != LobbyState.CountDown)
            {
                _localLobby.LocalLobbyState.Value = LobbyState.CountDown;
                SendLocalLobbyData();
            }
            else if (_localLobby.LocalLobbyState.Value == LobbyState.CountDown)
            {
                _localLobby.LocalLobbyState.Value = LobbyState.Lobby;
                SendLocalLobbyData();
            }
        }
        
        private void OnLobbyStateChanged(LobbyState state)
        {
            if (state == LobbyState.Lobby)
                CancelCountDown();
            if (state == LobbyState.CountDown) 
                BeginCountDown();
        }
        
        private void BeginCountDown()
        {
            Debug.Log("BeginCountDown");
            countDown.StartCountDown();
        }
        
        private void CancelCountDown()
        {
            Debug.Log("CancelCountDown");
            countDown.CancelCountDown();
        }

        public void FinishedCountDown()
        {
            _localUser.UserStatus.Value = PlayerStatus.InGame;
            _localLobby.LocalLobbyState.Value = LobbyState.InGame;
            _setupInGame.StartNetworkedGame(_localLobby, _localUser);
        }

        public void BeginGame()
        {
            if (_localUser.IsHost.Value)
            {
                _localLobby.LocalLobbyState.Value = LobbyState.CountDown;
                _localLobby.Locked.Value = true;
                SendLocalLobbyData();
            }
        }

        private void ClientQuitGame()
        {
            EndGame();
            _setupInGame.OnGameEnd();
        }

        public void EndGame()
        {
            if (_localUser.IsHost.Value)
            {
                _localLobby.LocalLobbyState.Value = LobbyState.Lobby;
                _localLobby.Locked.Value = false;
                SendLocalLobbyData();
            }
            
            // SetLobbyView();
        }

        #region Setup

        private async void Awake()
        {
            Application.wantsToQuit += OnWantToQuit;
            _localUser = new LocalPlayer("", 0, false, "LocalPlayer");
            _localLobby = new LocalLobby { LocalLobbyState = { Value = LobbyState.Lobby } };
            LobbyManager = new LobbyManager();
            
            await InitializeServices();
            AuthenticatePlayer();
        }
        
        private async Task InitializeServices()
        {
            string serviceProfileName = "player";
#if UNITY_EDITOR
            serviceProfileName = $"{serviceProfileName}_{ClonesManager.GetCurrentProject().name}";
#endif
            await Auth.Auth.Authenticate(serviceProfileName);
        }

        private void AuthenticatePlayer()
        {
            string localId = AuthenticationService.Instance.PlayerId;
            string randomName = NameGenerator.GetName(localId);
            
            _localUser.ID.Value = localId;
            _localUser.DisplayName.Value = randomName;
        }

        #endregion
        
        private void SetGameState(GameState state)
        {
            bool isLeavingLobby = state is GameState.Menu or GameState.JoinMenu &&
                                  LocalGameState == GameState.Lobby;
            LocalGameState = state;

            Debug.Log($"Switching Game State to : {LocalGameState}");

            if (isLeavingLobby)
                LeaveLobby();
            OnGameStateChanged.Invoke(LocalGameState);
        }

        private void SetCurrentLobbies(IEnumerable<LocalLobby> lobbies)
        {
            var newLobbyDict = lobbies.ToDictionary(lobby => lobby.LobbyID.Value);

            LocalLobbyList.CurrentLobbies = newLobbyDict;
            LocalLobbyList.QueryState.Value = LobbyQueryState.Fetched;
        }
        
        private async Task CreateLobby()
        {
            _localUser.IsHost.Value = true;
            // _localLobby.OnUserReadyChange = OnPlayersReady;
            try
            {
                await BindLobby();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Couldn't join Lobby: {exception}");
            }
        }
        
        private async Task JoinLobby()
        {
            _localUser.IsHost.ForceSet(false);
            await BindLobby();
        }

        private async Task BindLobby()
        {
            await LobbyManager.BindLocalLobbyToRemote(_localLobby.LobbyID.Value, _localLobby);
            _localLobby.LocalLobbyState.OnChanged += OnLobbyStateChanged;
            // SetLobbyView();
        }
        
        private void LeaveLobby()
        {
            _localUser.ResetState();
#pragma warning disable CS4014
            LobbyManager.LeaveLobbyAsync();
#pragma warning restore CS4014
            ResetLocalLobby();
        }

        private void SetLobbyView()
        {
            Debug.Log($"Setting Lobby user state {GameState.Lobby}");
            SetGameState(GameState.Lobby);
            SetLocalUserStatus(PlayerStatus.Lobby);
        }

        private void ResetLocalLobby()
        {
            _localLobby.ResetLobby();
            _localLobby.RelayServer = null;
        }
        
        #region Teardown

        /// <summary>
        /// In builds, if we are in a lobby and try to send a Leave request on application quit, it won't go through if we're quitting on the same frame.
        /// So, we need to delay just briefly to let the request happen (though we don't need to wait for the result).
        /// </summary>
        private IEnumerator LeaveBeforeQuit()
        {
            ForceLeaveAttempt();
            yield return null;
            Application.Quit();
        }

        private bool OnWantToQuit()
        {
            bool canQuit = string.IsNullOrEmpty(_localLobby?.LobbyID.Value);
            StartCoroutine(LeaveBeforeQuit());
            return canQuit;
        }

        private void OnDestroy()
        {
            ForceLeaveAttempt();
            LobbyManager.Dispose();
        }

        private void ForceLeaveAttempt()
        {
            if (!string.IsNullOrEmpty(_localLobby?.LobbyID.Value))
            {
#pragma warning disable 4014
                LobbyManager.LeaveLobbyAsync();
#pragma warning restore 4014
                _localLobby = null;
            }
        }

        #endregion
    }
}