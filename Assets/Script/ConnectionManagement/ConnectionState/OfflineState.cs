using Script.Lobby;
using Script.Networks;
using Script.Utils;
using UnityEngine.SceneManagement;
using VContainer;

namespace Script.ConnectionManagement.ConnectionState
{
    internal class OfflineState : ConnectionState
    {
        [Inject] private LobbyServiceFacade _lobbyServiceFacade;
        [Inject] private ProfileManager _profileManager;
        [Inject] private LocalLobby _localLobby;

        private const string MainMenuSceneName = "MainMenu";
        
        public override void Enter()
        {
            _lobbyServiceFacade.EndTracking();
            ConnectionManager.NetworkManager.Shutdown();
            if (SceneManager.GetActiveScene().name != MainMenuSceneName)
            {
                NetworkSceneManager.Instance.LoadScene(MainMenuSceneName, useNetworkSceneManager: false);
            }
        }

        public override void Exit() { }
        
        public override void StartClientIP(string playerName, string ipaddress, int port)
        {
            ConnectionMethodIP connectionMethod = new ConnectionMethodIP(ipaddress, (ushort)port, ConnectionManager, _profileManager, playerName);
            ConnectionManager.ClientReconnecting.Configure(connectionMethod);
            ConnectionManager.ChangeState(ConnectionManager.ClientConnecting.Configure(connectionMethod));
        }

        public override void StartClientLobby(string playerName)
        {
            ConnectionMethodRelay connectionMethod = new ConnectionMethodRelay(_lobbyServiceFacade, _localLobby, ConnectionManager, _profileManager, playerName);
            ConnectionManager.ClientReconnecting.Configure(connectionMethod);
            ConnectionManager.ChangeState(ConnectionManager.ClientConnecting.Configure(connectionMethod));
        }

        public override void StartHostIP(string playerName, string ipaddress, int port)
        {
            ConnectionMethodIP connectionMethod = new ConnectionMethodIP(ipaddress, (ushort)port, ConnectionManager, _profileManager, playerName);
            ConnectionManager.ChangeState(ConnectionManager.StartingHost.Configure(connectionMethod));
        }

        public override void StartHostLobby(string playerName)
        {
            ConnectionMethodRelay connectionMethod = new ConnectionMethodRelay(_lobbyServiceFacade, _localLobby, ConnectionManager, _profileManager, playerName);
            ConnectionManager.ChangeState(ConnectionManager.StartingHost.Configure(connectionMethod));
        }
    }
}