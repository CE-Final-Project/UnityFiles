using Script.Lobby;
using UnityEngine;
using VContainer;

namespace Script.ConnectionManagement.ConnectionState
{
    internal class ClientConnectedState : ConnectionState
    {
        [Inject] protected LobbyServiceFacade LobbyServiceFacade;
        public override void Enter()
        {
            if (LobbyServiceFacade.CurrentUnityLobby != null)
            {
                LobbyServiceFacade.BeginTracking();
            }
        }

        public override void Exit() { }
        
        public override void OnClientDisconnect(ulong _)
        {
            string disconnectReason = ConnectionManager.NetworkManager.DisconnectReason;
            if (string.IsNullOrEmpty(disconnectReason))
            {
                ConnectStatusPublisher.Publish(ConnectStatus.Reconnecting);
                ConnectionManager.ChangeState(ConnectionManager.ClientReconnecting);
            }
            else
            {
                ConnectStatus connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                ConnectStatusPublisher.Publish(connectStatus);
                ConnectionManager.ChangeState(ConnectionManager.Offline);
            }
        }

        public override void OnUserRequestedShutdown()
        {
            ConnectStatusPublisher.Publish(ConnectStatus.UserRequestedDisconnect);
            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }
    }
}