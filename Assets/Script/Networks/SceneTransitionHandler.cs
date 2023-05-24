using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.Networks
{
    public class SceneTransitionHandler : NetworkBehaviour
    {

        public static SceneTransitionHandler Instance { get; private set; }

        [SerializeField] private string defaultMainMenu = "MainMenu";

        public delegate void ClientLoadedSceneDelegateHandler(ulong clientId);

        public event ClientLoadedSceneDelegateHandler OnClientLoadedScene;

        public delegate void SceneStateChangedDelegateHandler(SceneStates newState);

        public event SceneStateChangedDelegateHandler OnSceneStateChanged;

        private int _numberOfClientLoaded;

        public enum SceneStates
        {
            Init,
            MainMenu,
            Lobby,
            InGame,
            PostGame
        }

        private SceneStates _sceneState;

        /// <summary>
        /// Awake
        /// If another version exists, destroy it and use the current version
        /// Set our scene state to INIT
        /// </summary>
        private void Awake()
        {
            if (Instance != this && Instance != null)
            {
                Destroy(Instance.gameObject);
            }

            Instance = this;
            SetSceneState(SceneStates.Init);
            DontDestroyOnLoad(this);
        }

        /// <summary>
        /// SetSceneState
        /// Sets the current scene state to help with transitioning.
        /// </summary>
        /// <param name="sceneState"></param>
        public void SetSceneState(SceneStates sceneState)
        {
            _sceneState = sceneState;
            if (OnSceneStateChanged != null)
            {
                OnSceneStateChanged.Invoke(_sceneState);
            }
        }

        /// <summary>
        /// GetCurrentSceneState
        /// Returns the current scene state
        /// </summary>
        /// <returns>current scene state</returns>
        public SceneStates GetCurrentSceneState()
        {
            return _sceneState;
        }

        /// <summary>
        /// Start
        /// Loads the default main menu when started (this should always be a component added to the networking manager)
        /// </summary>
        private void Start()
        {
            if (_sceneState == SceneStates.Init)
            {
                SceneManager.LoadScene(defaultMainMenu, LoadSceneMode.Single);
            }
        }

        /// <summary>
        /// Registers callbacks to the NetworkSceneManager. This should be called when starting the server
        /// </summary>
        public void RegisterCallbacks()
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        }

        /// <summary>
        /// Switches to a new scene
        /// </summary>
        /// <param name="sceneName"></param>
        public void SwitchScene(string sceneName)
        {
            if (NetworkManager.Singleton.IsListening)
            {
                _numberOfClientLoaded = 0;
                NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }
            else
            {
                SceneManager.LoadSceneAsync(sceneName);
            }
        }

        private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            _numberOfClientLoaded += 1;
            OnClientLoadedScene?.Invoke(clientId);
        }

        public bool AllClientsAreLoaded()
        {
            return _numberOfClientLoaded == NetworkManager.Singleton.ConnectedClients.Count;
        }

        /// <summary>
        /// ExitAndLoadStartMenu
        /// This should be invoked upon a user exiting a multiplayer game session.
        /// </summary>
        public void ExitAndLoadMainMenu()
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
            OnClientLoadedScene = null;
            SetSceneState(SceneStates.MainMenu);
            SceneManager.LoadScene(defaultMainMenu);
        }
    }
}
