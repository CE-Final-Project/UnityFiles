using Survival.Game.UnityServices.Lobbies;
using VContainer;

namespace Survival.Game.ConnectionManagement
{
    class ClientConnectedState : ConnectionState
    {
        [Inject] protected LobbyServiceFacade m_LobbyServiceFacade;
        
        public override void Enter()
        {
            if (m_LobbyServiceFacade.CurrentUnityLobby != null)
            {
                m_LobbyServiceFacade.BeginTracking();
            }
        }

        public override void Exit()
        {
        }

        public override void OnClientDisconnect(ulong _clientId)
        {
            m_ConnectStatusPublisher.Publish(ConnectStatus.Reconnecting);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientReconnecting);
        }
        
        public override void OnUserRequestedShutdown()
        {
            m_ConnectStatusPublisher.Publish(ConnectStatus.UserRequestedDisconnect);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }

        public override void OnDisconnectReasonReceived(ConnectStatus disconnectReason)
        {
            m_ConnectStatusPublisher.Publish(disconnectReason);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_DisconnectingWithReason);
        }
    }
}