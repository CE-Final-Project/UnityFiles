using System;
using Script.ConnectionManagement.ConnectionState;
using Script.Lobby;
using Script.Utils;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace Script.ConnectionManagement
{
    public abstract class ConnectionMethodBase
    {
        protected ConnectionManager ConnectionManager;
        private readonly ProfileManager _profileManager;
        protected readonly string PlayerName;
        
        public abstract Task SetupHostConnectionAsync();

        public abstract Task SetupClientConnectionAsync();

        protected ConnectionMethodBase(ConnectionManager connectionManager, ProfileManager profileManager, string playerName)
        {
            ConnectionManager = connectionManager;
            _profileManager = profileManager;
            PlayerName = playerName;
        }
        
        protected void SetConnectionPayload(string playerId, string playerName)
        {
            string payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                playerId = playerId,
                playerName = playerName,
                isDebug = Debug.isDebugBuild
            });

            byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            ConnectionManager.NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
        }
        
        protected string GetPlayerId()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                return ClientPrefs.GetGuid() + _profileManager.Profile;
            }
            
            return AuthenticationService.Instance.IsSignedIn ? AuthenticationService.Instance.PlayerId : ClientPrefs.GetGuid() + _profileManager.Profile;
        }
    }
    
    internal class ConnectionMethodIP : ConnectionMethodBase
    {
        private readonly string _ipaddress;
        private readonly ushort _port;
        
        public ConnectionMethodIP(string ip, ushort port, ConnectionManager connectionManager, ProfileManager profileManager, string playerName) 
            : base(connectionManager, profileManager, playerName)
        {
            _ipaddress = ip;
            _port = port;
            ConnectionManager = connectionManager;
        }

        public override Task SetupHostConnectionAsync()
        {
            SetConnectionPayload(GetPlayerId(), PlayerName); // Need to set connection payload for host as well, as host is a client too
            UnityTransport utp = (UnityTransport)ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(_ipaddress, _port);
            return Task.CompletedTask;
        }

        public override Task SetupClientConnectionAsync()
        {
            SetConnectionPayload(GetPlayerId(), PlayerName); 
            UnityTransport utp = (UnityTransport)ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(_ipaddress, _port);
            return Task.CompletedTask;
        }
    }
    
    internal class ConnectionMethodRelay : ConnectionMethodBase
    {
        private readonly LobbyServiceFacade _lobbyServiceFacade;
        private readonly LocalLobby _localLobby;

        public ConnectionMethodRelay(LobbyServiceFacade lobbyServiceFacade, LocalLobby localLobby, ConnectionManager connectionManager, ProfileManager profileManager, string playerName) 
            : base(connectionManager, profileManager, playerName)
        {
            _lobbyServiceFacade = lobbyServiceFacade;
            _localLobby = localLobby;
            ConnectionManager = connectionManager;
        }

        public override async Task SetupHostConnectionAsync()
        {
            Debug.Log("Setting up Unity Relay host");

            SetConnectionPayload(GetPlayerId(), PlayerName); // Need to set connection payload for host as well, as host is a client too

            // Create relay allocation
            Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(ConnectionManager.MaxConnectedPlayers, region: null);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            Debug.Log($"server: connection data: {hostAllocation.ConnectionData[0]} {hostAllocation.ConnectionData[1]}, " +
                      $"allocation ID:{hostAllocation.AllocationId}, region:{hostAllocation.Region}");

            _localLobby.RelayJoinCode = joinCode;
            _localLobby.RelayRegion = hostAllocation.Region;

            //next line enable lobby and relay services integration
            await _lobbyServiceFacade.UpdateLobbyDataAsync(_localLobby.GetDataForUnityServices());
            await _lobbyServiceFacade.UpdatePlayerRelayInfoAsync(hostAllocation.AllocationIdBytes.ToString(), joinCode);

            // Setup UTP with relay connection info
            UnityTransport utp = (UnityTransport)ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetRelayServerData(new RelayServerData(hostAllocation, OnlineState.DtlsConnType)); // This is with DTLS enabled for a secure connection
        }

        public override async Task SetupClientConnectionAsync()
        {
            Debug.Log("Setting up Unity Relay client");

            SetConnectionPayload(GetPlayerId(), PlayerName);

            if (_lobbyServiceFacade.CurrentUnityLobby == null)
            {
                throw new Exception("Trying to start relay while Lobby isn't set");
            }

            Debug.Log($"Setting Unity Relay client with join code {_localLobby.RelayJoinCode}");

            // Create client joining allocation from join code
            JoinAllocation joinedAllocation = await RelayService.Instance.JoinAllocationAsync(_localLobby.RelayJoinCode);
            Debug.Log($"client: {joinedAllocation.ConnectionData[0]} {joinedAllocation.ConnectionData[1]}, " +
                      $"host: {joinedAllocation.HostConnectionData[0]} {joinedAllocation.HostConnectionData[1]}, " +
                      $"client: {joinedAllocation.AllocationId}");

            await _lobbyServiceFacade.UpdatePlayerRelayInfoAsync(joinedAllocation.AllocationId.ToString(), _localLobby.RelayJoinCode);

            // Configure UTP with allocation
            UnityTransport utp = (UnityTransport)ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetRelayServerData(new RelayServerData(joinedAllocation, OnlineState.DtlsConnType));
        }
    }
}