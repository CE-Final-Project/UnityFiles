using System;
using Script.Game.GameplayObject.Character;
using Script.Utils;
using Unity.Multiplayer.Samples.BossRoom;
using UnityEngine;

namespace Script.ConnectionManagement
{
    public struct SessionPlayerData : ISessionPlayerData
    {
        
        public string PlayerName;
        public int PlayerNumber;
        public Vector2 PlayerPosition;
        public bool IsPlayerFlipped;
        public Guid AvatarGuid;
        public int CurrentHitPoints;
        public bool HasCharacterSpawned;

        public SessionPlayerData(ulong clientID, string name, Guid avatarGuid, int currentHitPoints = 0, bool isConnected = false, bool hasCharacterSpawned = false)
        {
            ClientID = clientID;
            PlayerName = name;
            PlayerNumber = -1;
            PlayerPosition = Vector2.zero;
            IsPlayerFlipped = false;
            AvatarGuid = avatarGuid;
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