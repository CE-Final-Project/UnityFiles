using System;
using Script.Configuration;
using Script.Utils;
using Unity.Netcode;

namespace Script.GameState
{
    public class NetworkCharSelection : NetworkBehaviour
    {
        public enum SeatState : byte
        {
            Inactive,
            Active,
            LockedIn,
        }

        public struct LobbyPlayerState : INetworkSerializable, IEquatable<LobbyPlayerState>
        {

            public ulong ClientId;

            private FixedPlayerName _playerName;

            public int PlayerNumber;
            public int SeatIdx;
            public float LastChangeTime;
            
            public SeatState SeatState;
            
            public LobbyPlayerState(ulong clientId, string name, int playerNumber, SeatState state, int seatIdx = -1, float lastChangeTime = 0)
            {
                ClientId = clientId;
                PlayerNumber = playerNumber;
                SeatState = state;
                SeatIdx = seatIdx;
                LastChangeTime = lastChangeTime;
                _playerName = new FixedPlayerName();
                
                PlayerName = name;
            }

            public string PlayerName
            {
                get => _playerName;
                private set => _playerName = value;
            }
            
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref ClientId);
                serializer.SerializeValue(ref _playerName);
                serializer.SerializeValue(ref PlayerNumber);
                serializer.SerializeValue(ref SeatState);
                serializer.SerializeValue(ref SeatIdx);
                serializer.SerializeValue(ref LastChangeTime);
            }

            public bool Equals(LobbyPlayerState other)
            {
                return ClientId == other.ClientId &&
                       PlayerName == other.PlayerName &&
                       PlayerNumber == other.PlayerNumber &&
                       SeatState == other.SeatState &&
                       SeatIdx == other.SeatIdx &&
                       LastChangeTime.Equals(other.LastChangeTime);
            }
        }

        private NetworkList<LobbyPlayerState> _lobbyPlayerStates;

        public Avatar[] AvatarConfiguration;
        
        private void Awake()
        {
            _lobbyPlayerStates = new NetworkList<LobbyPlayerState>();
        }
        
        public NetworkList<LobbyPlayerState> LobbyPlayerStates => _lobbyPlayerStates;

        public NetworkVariable<bool> IsLobbyClosed { get; } = new NetworkVariable<bool>(false);

        public event Action<ulong, int, bool> OnClientChangedSeat;

        [ServerRpc(RequireOwnership = false)]
        public void ChangeSeatServerRpc(ulong clientId, int seatIdx, bool lockedIn)
        {
            OnClientChangedSeat?.Invoke(clientId, seatIdx, lockedIn);
        }
    }
}