using Survival.Game.Infrastructure;
using Unity.Multiplayer.Samples.BossRoom;
using UnityEngine;

namespace Survival.Game.ConnectionManagement
{
    public struct SessionPlayerData: ISessionPlayerData
    {
        public string PlayerName;
        public int PlayerNumber;
        public Vector3 PlayerPosition;
        // Instead of using a NetworkGuid (2 ulong) we could just use an int or even a byte-sized index into an array of possible avatars defined in our game data source
        public NetworkGuid AvatarNetworkGuid;
        public int CurrentHitPoints;
        public bool HasCharacterSpawned;

        public SessionPlayerData(ulong clientID, string name, NetworkGuid avatarNetworkGuid, int currentHitPoints = 0,
            bool isConnected = false, bool hasCharacterSpawned = false)
        {
            ClientID = clientID;
            PlayerName = name;
            PlayerNumber = -1;
            PlayerPosition = Vector3.zero;
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