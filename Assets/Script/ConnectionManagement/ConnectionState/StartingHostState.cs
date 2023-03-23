using System;
using Script.Lobby;
using Script.Utils;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Script.ConnectionManagement.ConnectionState
{
    internal class StartingHostState : OnlineState
    {
        [Inject] private LobbyServiceFacade _lobbyServiceFacade;
        [Inject] private LocalLobby _localLobby;
        
        private ConnectionMethodBase _connectionMethod;
        
        public StartingHostState Configure(ConnectionMethodBase baseConnectionMethod)
        {
            _connectionMethod = baseConnectionMethod;
            return this;
        }
        
        public override void Enter()
        {
            StartHost();
        }

        public override void Exit() { }
        
        public override void OnClientDisconnect(ulong clientId)
        {
            if (clientId == ConnectionManager.NetworkManager.LocalClientId)
            {
                StartHostFailed();
            }
        }

        private void StartHostFailed()
        {
            ConnectStatusPublisher.Publish(ConnectStatus.StartHostFailed);
            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }

        public override void OnServerStarted()
        {
            ConnectStatusPublisher.Publish(ConnectStatus.Success);
            ConnectionManager.ChangeState(ConnectionManager.Hosting);
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            byte[] connectionData = request.Payload;
            ulong clientId = request.ClientNetworkId;
            // This happens when starting as a host, before the end of the StartHost call. In that case, we simply approve ourselves.
            if (clientId == ConnectionManager.NetworkManager.LocalClientId)
            {
                string payload = System.Text.Encoding.UTF8.GetString(connectionData);
                ConnectionPayload connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload); // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html

                SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId, connectionPayload.playerId,
                    new SessionPlayerData(clientId, connectionPayload.playerName, new NetworkGuid(), 0, true));

                // connection approval will create a player object for you
                response.Approved = true;
                response.CreatePlayerObject = true;
            }
        }

        async void StartHost()
        {
            try
            {
                await _connectionMethod.SetupHostConnectionAsync();
                Debug.Log($"Created relay allocation with join code {_localLobby.RelayJoinCode}");

                // NGO's StartHost launches everything
                if (!ConnectionManager.NetworkManager.StartHost())
                {
                    OnClientDisconnect(ConnectionManager.NetworkManager.LocalClientId);
                }
            }
            catch (Exception)
            {
                StartHostFailed();
                throw;
            }
        }
    }
}