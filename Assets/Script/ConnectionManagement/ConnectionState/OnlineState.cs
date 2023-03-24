namespace Script.ConnectionManagement.ConnectionState
{
    internal abstract class OnlineState : ConnectionState
    {
        public const string DtlsConnType = "dtls";

        public override void OnUserRequestedShutdown()
        {
            // This behaviour will be the same for every online state
            ConnectStatusPublisher.Publish(ConnectStatus.UserRequestedDisconnect);
            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }

        public override void OnTransportFailure()
        {
            // This behaviour will be the same for every online state
            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }
    }
}