namespace Survival.Game.ConnectionManagement
{
    class DisconnectingWithReasonState : OnlineState
    {
        public override void Enter() { }

        public override void Exit() { }

        public override void OnClientDisconnect(ulong _clientId)
        {
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }
    }
}