using System;
using System.Collections.Generic;
using Script.ConnectionManagement.ConnectionState;
using Script.Utils;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Script.ConnectionManagement
{
    
    public enum ConnectStatus
    {
        Undefined,
        Success,                  //client successfully connected. This may also be a successful reconnect.
        ServerFull,               //can't join, server is already at capacity.
        LoggedInAgain,            //logged in on a separate client, causing this one to be kicked out.
        UserRequestedDisconnect,  //Intentional Disconnect triggered by the user.
        GenericDisconnect,        //server disconnected, but no specific reason given.
        Reconnecting,             //client lost connection and is attempting to reconnect.
        IncompatibleBuildType,    //client build type is incompatible with server.
        HostEndedSession,         //host intentionally ended the session.
        StartHostFailed,          // server failed to bind
        StartClientFailed         // failed to connect to server and/or invalid network endpoint
    }
    
    public struct ReconnectMessage
    {
        public int CurrentAttempt;
        public int MaxAttempt;

        public ReconnectMessage(int currentAttempt, int maxAttempt)
        {
            CurrentAttempt = currentAttempt;
            MaxAttempt = maxAttempt;
        }
    }

    public struct ConnectionEventMessage : INetworkSerializeByMemcpy
    {
        public ConnectStatus ConnectStatus;
        public FixedPlayerName PlayerName;
    }

    [Serializable]
    public class ConnectionPayload
    {
        public string playerId;
        public string playerName;
        public bool isDebug;
    }
    
    public class ConnectionManager : MonoBehaviour
    {
        private ConnectionState.ConnectionState _currentState;
        
        [Inject] private NetworkManager _networkManager;
        public NetworkManager NetworkManager => _networkManager;
        
        [SerializeField] private int numberOfReconnectAttempts = 2;
        public int NumberOfReconnectAttempts => numberOfReconnectAttempts;

        [Inject] private IObjectResolver _resolver;

        public int MaxConnectedPlayers = 4;

        internal readonly OfflineState Offline = new OfflineState();
        internal readonly ClientConnectingState ClientConnecting = new ClientConnectingState();
        internal readonly ClientConnectedState ClientConnected = new ClientConnectedState();
        internal readonly ClientReconnectingState ClientReconnecting = new ClientReconnectingState();
        internal readonly StartingHostState StartingHost = new StartingHostState();
        internal readonly HostingState Hosting = new HostingState();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            List<ConnectionState.ConnectionState> states = new List<ConnectionState.ConnectionState>
            {
                Offline,
                ClientConnecting,
                ClientConnected,
                ClientReconnecting,
                StartingHost,
                Hosting
            };

            foreach (ConnectionState.ConnectionState state in states)
            {
                _resolver.Inject(state);
            }
            
            _currentState = Offline;
            
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            NetworkManager.OnServerStarted += OnServerStarted;
            NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.OnTransportFailure += OnTransportFailure;
        }

        private void OnDestroy()
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            NetworkManager.OnServerStarted -= OnServerStarted;
            NetworkManager.ConnectionApprovalCallback -= ApprovalCheck;
            NetworkManager.OnTransportFailure -= OnTransportFailure;
        }

        internal void ChangeState(ConnectionState.ConnectionState nextState)
        {
            Debug.Log($"{name}: Changed connection state from {_currentState.GetType().Name} to {nextState.GetType().Name}.");

            if (_currentState != null)
            {
                _currentState.Exit();
            }
            _currentState = nextState;
            _currentState.Enter();
        }

        private void OnClientDisconnectCallback(ulong clientId)
        {
            _currentState.OnClientDisconnect(clientId);
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            _currentState.OnClientConnected(clientId);
        }

        private void OnServerStarted()
        {
            _currentState.OnServerStarted();
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            _currentState.ApprovalCheck(request, response);
        }

        private void OnTransportFailure()
        {
            _currentState.OnTransportFailure();
        }

        public void StartClientLobby(string playerName)
        {
            _currentState.StartClientLobby(playerName);
        }

        public void StartClientIp(string playerName, string ipaddress, int port)
        {
            _currentState.StartClientIP(playerName, ipaddress, port);
        }

        public void StartHostLobby(string playerName)
        {
            _currentState.StartHostLobby(playerName);
        }

        public void StartHostIp(string playerName, string ipaddress, int port)
        {
            _currentState.StartHostIP(playerName, ipaddress, port);
        }

        public void RequestShutdown()
        {
            _currentState.OnUserRequestedShutdown();
        }
    }
}