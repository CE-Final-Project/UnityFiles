using System.Numerics;
using Script.Utils;
using Unity.Multiplayer.Samples.BossRoom;

namespace Script.ConnectionManagement
{
    public struct SessionPlayerData : ISessionPlayerData
    {
        
        public string PlayerName;
        public int PlayerNumber;
        public Vector2 PlayerPosition;
        public bool IsPlayerFlipped;
        public NetworkGuid AvatarNetworkGuid;
        public int CurrentHitPoints;
        public bool HasCharacterSpawned;

        public SessionPlayerData(ulong clientID, string name, NetworkGuid avatarNetworkGuid, int currentHitPoints = 0, bool isConnected = false, bool hasCharacterSpawned = false)
        {
            ClientID = clientID;
            PlayerName = name;
            PlayerNumber = -1;
            PlayerPosition = Vector2.Zero;
            IsPlayerFlipped = false;
            AvatarNetworkGuid = avatarNetworkGuid;
            CurrentHitPoints = currentHitPoints;
            IsConnected = isConnected;
            HasCharacterSpawned = hasCharacterSpawned;
        }
        
        public bool IsConnected { get; set; }
        public ulong ClientID { get; set; }
        public void Reinitialize()
        {
            HasCharacterSpawned = false;
        }
    }
}