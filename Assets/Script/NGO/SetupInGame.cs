using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Script.Game;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Script.NGO
{
    public class SetupInGame : MonoBehaviour
    {
        [SerializeField] private GameObject inGameRunnerPrefab;
        [SerializeField] private GameObject[] disableWhileInGame;
        
        private InGameRunner _inGameRunner;
        
        private bool _doesNeedCleanup = false;
        private bool _hasConnectedViaNgo = false;

        private LocalLobby _lobby;

        private void SetMenuVisibility(bool areVisible)
        {
            foreach (GameObject go in disableWhileInGame)
                go.SetActive(areVisible);
        }

        /// <summary>
        /// The prefab with the NetworkManager contains all of the assets and logic needed to set up the NGO minigame.
        /// The UnityTransport needs to also be set up with a new Allocation from Relay.
        /// </summary>
        private async Task CreateNetworkManager(LocalLobby localLobby, LocalPlayer localPlayer)
        {
            _lobby = localLobby;
            _inGameRunner = Instantiate(inGameRunnerPrefab).GetComponentInChildren<InGameRunner>();
            _inGameRunner.Initialize(OnConnectionVerified, _lobby.PlayerCount, OnGameBegin, OnGameEnd,
                localPlayer);
            if (localPlayer.IsHost.Value)
            {
                await SetRelayHostData();
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                await AwaitRelayCode(localLobby);
                await SetRelayClientData();
                NetworkManager.Singleton.StartClient();
            }
        }

        private static async Task AwaitRelayCode(LocalLobby lobby)
        {
            string relayCode = lobby.RelayCode.Value;
            lobby.RelayCode.OnChanged += (code) => relayCode = code;
            while (string.IsNullOrEmpty(relayCode))
            {
                await Task.Delay(100);
            }
        }

        private async Task SetRelayHostData()
        {
            UnityTransport transport = NetworkManager.Singleton.GetComponentInChildren<UnityTransport>();

            Allocation allocation = await Relay.Instance.CreateAllocationAsync(_lobby.MaxPlayerCount.Value);
            string joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            GameManager.Instance.HostSetRelayCode(joinCode);

            NetworkEndPoint endpoint = GetEndpointForAllocation(allocation.ServerEndpoints,
                allocation.RelayServer.IpV4, allocation.RelayServer.Port, out bool isSecure);

            transport.SetHostRelayData(AddressFromEndpoint(endpoint), endpoint.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, isSecure);
        }

        private async Task SetRelayClientData()
        {
            UnityTransport transport = NetworkManager.Singleton.GetComponentInChildren<UnityTransport>();

            JoinAllocation joinAllocation = await Relay.Instance.JoinAllocationAsync(_lobby.RelayCode.Value);
            NetworkEndPoint endpoint = GetEndpointForAllocation(joinAllocation.ServerEndpoints,
                joinAllocation.RelayServer.IpV4, joinAllocation.RelayServer.Port, out bool isSecure);

            transport.SetClientRelayData(AddressFromEndpoint(endpoint), endpoint.Port,
                joinAllocation.AllocationIdBytes, joinAllocation.Key,
                joinAllocation.ConnectionData, joinAllocation.HostConnectionData, isSecure);
        }

        /// <summary>
        /// Determine the server endpoint for connecting to the Relay server, for either an Allocation or a JoinAllocation.
        /// If DTLS encryption is available, and there's a secure server endpoint available, use that as a secure connection. Otherwise, just connect to the Relay IP unsecured.
        /// </summary>
        private static NetworkEndPoint GetEndpointForAllocation(
            IEnumerable<RelayServerEndpoint> endpoints,
            string ip,
            int port,
            out bool isSecure)
        {
#if ENABLE_MANAGED_UNITYTLS
            RelayServerEndpoint secureEndPoint = endpoints.FirstOrDefault(f => f.Secure && f.Network == RelayServerEndpoint.NetworkOptions.Udp);
            
            if (secureEndPoint != null)
            {
                isSecure = true;
                return NetworkEndPoint.Parse(secureEndPoint.Host, (ushort)secureEndPoint.Port);
            }
#endif
            isSecure = false;
            return NetworkEndPoint.Parse(ip, (ushort)port);
        }

        static string AddressFromEndpoint(NetworkEndPoint endpoint)
        {
            return endpoint.Address.Split(':')[0];
        }

        private void OnConnectionVerified()
        {
            _hasConnectedViaNgo = true;
        }

        public void StartNetworkedGame(LocalLobby localLobby, LocalPlayer localPlayer)
        {
            _doesNeedCleanup = true;
            SetMenuVisibility(false);
#pragma warning disable 4014
            CreateNetworkManager(localLobby, localPlayer);
#pragma warning restore 4014
        }

        public void OnGameBegin()
        {
            if (!_hasConnectedViaNgo)
            {
                // If this localPlayer hasn't successfully connected via NGO, forcibly exit the minigame.
                // LogHandlerSettings.Instance.SpawnErrorPopup("Failed to join the game.");
                OnGameEnd();
            }
        }

        /// <summary>
        /// Return to the localLobby after the game, whether due to the game ending or due to a failed connection.
        /// </summary>
        public void OnGameEnd()
        {
            if (_doesNeedCleanup)
            {
                NetworkManager.Singleton.Shutdown(true);
                Destroy(_inGameRunner
                    .transform.parent
                    .gameObject); // Since this destroys the NetworkManager, that will kick off cleaning up networked objects.
                SetMenuVisibility(true);
                _lobby.RelayCode.Value = "";
                GameManager.Instance.EndGame();
                _doesNeedCleanup = false;
            }
        }
    }
}