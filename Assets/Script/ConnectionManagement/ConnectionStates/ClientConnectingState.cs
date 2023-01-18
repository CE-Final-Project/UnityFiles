using System;
using System.Threading.Tasks;
using Survival.Game.UnityServices.Lobbies;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using VContainer;

namespace Survival.Game.ConnectionManagement
{
    class ClientConnectingState : OnlineState
    {
        [Inject] protected LobbyServiceFacade m_LobbyServiceFacade;
        [Inject] protected LocalLobby m_LocalLobby;

        public override void Enter()
        {
#pragma warning disable 4014
            ConnectClientAsync();
#pragma warning restore 4014
        }

        public override void Exit()
        {
            
        }

        public override void OnClientConnected(ulong _clientId)
        {
            m_ConnectStatusPublisher.Publish(ConnectStatus.Success);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnected);
        }

        public override void OnClientDisconnect(ulong _clientId)
        {
            m_ConnectStatusPublisher.Publish(ConnectStatus.StartClientFailed);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }
        
     public override void OnDisconnectReasonReceived(ConnectStatus disconnectReason)
        {
            m_ConnectStatusPublisher.Publish(disconnectReason);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_DisconnectingWithReason);
        }

        protected async Task ConnectClientAsync()
        {
            bool success = true;
            if (m_LobbyServiceFacade.CurrentUnityLobby != null)
            {
                success = await JoinRelayServerAsync();
            }

            if (success)
            {
                success = m_ConnectionManager.NetworkManager.StartClient();
            }

            if (success)
            {
                SceneLoaderWrapper.Instance.AddOnSceneEventCallback();
                m_ConnectionManager.RegisterCustomMessages();
            }
            else
            {
                OnClientDisconnect(0);
            }
        }

        async Task<bool> JoinRelayServerAsync()
        {
            Debug.Log($"Setting Unity Relay client with join code {m_LocalLobby.RelayJoinCode}");

            try
            {
                var (ipv4Address, port, allocationIdBytes, allocationId, connectionData, hostConnectionData, key) =
                    await UnityRelayUtilities.JoinRelayServerFromJoinCode(m_LocalLobby.RelayJoinCode);

                await m_LobbyServiceFacade.UpdatePlayerRelayInfoAsync(allocationId.ToString(), m_LocalLobby.RelayJoinCode);
                var utp = (UnityTransport)m_ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
                utp.SetClientRelayData(ipv4Address, port, allocationIdBytes, key, connectionData, hostConnectionData, isSecure: true);
            }
            catch (Exception e)
            {
                Debug.Log($"Relay join failed: {e.Message}");
                //leave the lobby if relay failed for some reason
                await m_LobbyServiceFacade.EndTracking();
                return false;
            }

            return true;
        }
    }
}