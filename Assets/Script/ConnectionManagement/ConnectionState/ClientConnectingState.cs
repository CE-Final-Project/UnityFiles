using System;
using System.Threading.Tasks;
using Script.Lobby;
using Script.Networks;
using UnityEngine;
using VContainer;

namespace Script.ConnectionManagement.ConnectionState
{
    internal class ClientConnectingState : OnlineState
    {
        [Inject] protected LobbyServiceFacade LobbyServiceFacade;
        [Inject] protected LocalLobby LocalLobby;
        
        private ConnectionMethodBase _connectionMethod;

        public ClientConnectingState Configure(ConnectionMethodBase baseConnectionMethod)
        {
            _connectionMethod = baseConnectionMethod;
            return this;
        }
        
        public override void Enter()
        {
#pragma warning disable 4014
            ConnectClientAsync();
#pragma warning restore 4014
        }

        public override void Exit() { }
        
        public override void OnClientConnected(ulong _)
        {
            ConnectStatusPublisher.Publish(ConnectStatus.Success);
            ConnectionManager.ChangeState(ConnectionManager.ClientConnected);
        }

        public override void OnClientDisconnect(ulong _)
        {
            // client ID is for sure ours here
            StartingClientFailedAsync();
        }

        protected void StartingClientFailedAsync()
        {
            var disconnectReason = ConnectionManager.NetworkManager.DisconnectReason;
            if (string.IsNullOrEmpty(disconnectReason))
            {
                ConnectStatusPublisher.Publish(ConnectStatus.StartClientFailed);
            }
            else
            {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                ConnectStatusPublisher.Publish(connectStatus);
            }
            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }


        internal async Task ConnectClientAsync()
        {
            try
            {
                // Setup NGO with current connection method
                await _connectionMethod.SetupClientConnectionAsync();

                // NGO's StartClient launches everything
                if (!ConnectionManager.NetworkManager.StartClient())
                {
                    throw new Exception("NetworkManager StartClient failed");
                }

                NetworkSceneManager.Instance.AddOnSceneEventCallback();
            }
            catch (Exception e)
            {
                Debug.LogError("Error connecting client, see following exception");
                Debug.LogException(e);
                StartingClientFailedAsync();
                throw;
            }
        }
    }
}