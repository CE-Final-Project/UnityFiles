using System;
using System.Collections;
using Script.ApplicationLifecycle.Messages;
using Script.ConnectionManagement;
using Script.Game.Messages;
using Script.GameState;
using Script.Infrastructure;
using Script.Infrastructure.PubSub;
using Script.Lobby;
using Script.Networks;
using Script.NGO;
using Script.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Script.ApplicationLifecycle
{
    public class ApplicationController : LifetimeScope
    {
        [SerializeField] private UpdateRunner updateRunner;
        [SerializeField] private ConnectionManager connectionManager;
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private EnemySpawner enemySpawner;
        
        private LocalLobby _localLobby;
        private LobbyServiceFacade _lobbyServiceFacade;

        private IDisposable _subscriptions;
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterComponent(updateRunner);
            builder.RegisterComponent(connectionManager);
            builder.RegisterComponent(networkManager);
            builder.RegisterComponent(enemySpawner);

            builder.Register<LocalLobbyUser>(Lifetime.Singleton);
            builder.Register<LocalLobby>(Lifetime.Singleton);

            builder.Register<ProfileManager>(Lifetime.Singleton);

            builder.Register<PersistantGameState>(Lifetime.Singleton);
            

            builder.RegisterInstance(new MessageChannel<QuitApplicationMessage>()).AsImplementedInterfaces();
            builder.RegisterInstance(new MessageChannel<UnityServiceErrorMessage>()).AsImplementedInterfaces();
            builder.RegisterInstance(new MessageChannel<ConnectStatus>()).AsImplementedInterfaces();

            builder.RegisterComponent(new NetworkedMessageChannel<LifeStateChangedEventMessage>()).AsImplementedInterfaces();
            builder.RegisterComponent(new NetworkedMessageChannel<ConnectionEventMessage>()).AsImplementedInterfaces();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            builder.RegisterComponent(new NetworkedMessageChannel<CheatUsedMessage>()).AsImplementedInterfaces();
#endif
                
            //this message channel is essential and persists for the lifetime of the lobby and relay services
            builder.RegisterInstance(new MessageChannel<ReconnectMessage>()).AsImplementedInterfaces();

            //buffered message channels hold the latest received message in buffer and pass to any new subscribers
            builder.RegisterInstance(new BufferedMessageChannel<LobbyListFetchedMessage>()).AsImplementedInterfaces();

            //all the lobby service stuff, bound here so that it persists through scene loads
            builder.Register<AuthenticationServiceFacade>(Lifetime.Singleton); //a manager entity that allows us to do anonymous authentication with unity services

            //LobbyServiceFacade is registered as entrypoint because it wants a callback after container is built to do it's initialization
            builder.RegisterEntryPoint<LobbyServiceFacade>(Lifetime.Singleton).AsSelf();
        }

        public void Start()
        {
            _localLobby = Container.Resolve<LocalLobby>();
            _lobbyServiceFacade = Container.Resolve<LobbyServiceFacade>();

            var quitApplicationSub = Container.Resolve<ISubscriber<QuitApplicationMessage>>();
                
            DisposableGroup subHandles = new DisposableGroup();
            subHandles.Add(quitApplicationSub.Subscribe(QuitGame));
            _subscriptions = subHandles;
            
            Application.wantsToQuit += OnWantsToQuit;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(updateRunner.gameObject);
            Application.targetFrameRate = 60;
            SceneManager.LoadScene("MainMenu");
        }
        
        protected override void OnDestroy()
        {
            _subscriptions?.Dispose();
            _lobbyServiceFacade?.EndTracking();
            base.OnDestroy();
        }
        
        private IEnumerator LeaveBeforeQuit()
        {
            try
            {
                _lobbyServiceFacade.EndTracking();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            yield return null;
            Application.Quit();
        }

        private bool OnWantsToQuit()
        {
            bool canQuit = string.IsNullOrEmpty(_localLobby?.LobbyID);
            if (!canQuit)
            {
                StartCoroutine(LeaveBeforeQuit());
            }
            
            return canQuit;
        }
        
        private static void QuitGame(QuitApplicationMessage msg)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

    }
}