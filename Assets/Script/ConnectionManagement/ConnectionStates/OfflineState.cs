using Survival.Game.UnityServices.Lobbies;
using Survival.Game.Utils;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Survival.Game.ConnectionManagement
{
    class OfflineState: ConnectionState
    {
        [Inject]
        LobbyServiceFacade m_LobbyServiceFacade;
        [Inject]
        ProfileManager m_ProfileManager;

        const string k_MainMenuSceneName = "MainMenu";
        
        
        public override void Enter()
        {
            m_LobbyServiceFacade.EndTracking();
            m_ConnectionManager.NetworkManager.Shutdown();
            if (SceneManager.GetActiveScene().name != k_MainMenuSceneName)
            {
                SceneLoaderWrapper.Instance.LoadScene(k_MainMenuSceneName, useNetworkSceneManager: false);
            }
        }

        public override void Exit()
        {
            // Offline do noting
        }

        public override void StartClientIP(string playerName, string ipaddress, int port)
        {
            var utp = (UnityTransport)m_ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(ipaddress, (ushort) port);
            SetConnectionPayload(GetPlayerId(), playerName);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnecting);
        }
        
        public override void StartClientLobby(string playerName)
        {
            SetConnectionPayload(GetPlayerId(), playerName);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnecting);
        }

        public override void StartHostIP(string playerName, string ipaddress, int port)
        {
            var utp = (UnityTransport)m_ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(ipaddress, (ushort)port);

            SetConnectionPayload(GetPlayerId(), playerName);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_StartingHost);
        }

        public override void StartHostLobby(string playerName)
        {
            SetConnectionPayload(GetPlayerId(), playerName);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_StartingHost);
        }

        void SetConnectionPayload(string playerId, string playerName)
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                playerId = playerId,
                playerName = playerName,
                isDebug = Debug.isDebugBuild
            });

            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            m_ConnectionManager.NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
        }

        string GetPlayerId()
        {
            if (Unity.Services.Core.UnityServices.State != ServicesInitializationState.Initialized)
            {
                return ClientPrefs.GetGuid() + m_ProfileManager.Profile;
            }

            return AuthenticationService.Instance.IsSignedIn ? AuthenticationService.Instance.PlayerId : ClientPrefs.GetGuid() + m_ProfileManager.Profile;
        }
    }
}