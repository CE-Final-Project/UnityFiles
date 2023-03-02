using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Script.Infrastructure;
using UnityEngine;

namespace Script
{
    public enum LobbyState
    {
        Lobby = 1,
        CountDown = 2,
        InGame = 3
    }
    
    [Serializable]
    public class LocalLobby
    {
        public Action<LocalPlayer> OnUserJoined;
        public Action<int> OnUserLeft;
        public Action<int> OnUserReadyChange;
        public CallbackValue<string> LobbyID = new CallbackValue<string>();
        public CallbackValue<string> LobbyCode = new CallbackValue<string>();
        public CallbackValue<string> RelayCode = new CallbackValue<string>();
        public CallbackValue<ServerAddress> RelayServer = new CallbackValue<ServerAddress>();
        public CallbackValue<string> LobbyName = new CallbackValue<string>();
        public CallbackValue<string> HostID = new CallbackValue<string>();
        public CallbackValue<LobbyState> LocalLobbyState = new CallbackValue<LobbyState>();
        public CallbackValue<bool> Locked = new CallbackValue<bool>();
        public CallbackValue<bool> Private = new CallbackValue<bool>();
        public CallbackValue<int> AvailableSlots = new CallbackValue<int>();
        public CallbackValue<int> MaxPlayerCount = new CallbackValue<int>();
        // public CallbackValue<LobbyColor> LocalLobbyColor = new CallbackValue<LobbyColor>();
        public CallbackValue<long> LastUpdated = new CallbackValue<long>();

        private List<LocalPlayer> _localPlayers = new List<LocalPlayer>();
        
        public int PlayerCount => _localPlayers.Count;
        private ServerAddress _relayServer;
        
        public IReadOnlyList<LocalPlayer> LocalPlayers => _localPlayers;
        
        public void ResetLobby()
        {
            _localPlayers.Clear();
            LobbyName.Value = "";
            LobbyID.Value = "";
            LobbyCode.Value = "";
            Locked.Value = false;
            Private.Value = false;
            // LocalLobbyColor.Value = LobbyRelaySample.LobbyColor.None;
            AvailableSlots.Value = 4;
            MaxPlayerCount.Value = 4;
            OnUserJoined = null;
            OnUserLeft = null;
        }
        
        public LocalLobby()
        {
            LastUpdated.Value = DateTime.Now.ToFileTimeUtc();
        }
        
        public LocalPlayer GetLocalPlayer(int index)
        {
            return PlayerCount > index ? _localPlayers[index] : null;
        }
        
        public void AddPlayer(int index, LocalPlayer user)
        {
            _localPlayers.Insert(index, user);
            user.UserStatus.OnChanged += OnUserChangedStatus;
            OnUserJoined?.Invoke(user);
            Debug.Log($"Added User: {user.DisplayName.Value} - {user.ID.Value} to slot {index + 1}/{PlayerCount}");
        }
        
        public void RemovePlayer(int playerIndex)
        {
            if (PlayerCount <= playerIndex) return;
            LocalPlayer user = _localPlayers[playerIndex];
            user.UserStatus.OnChanged -= OnUserChangedStatus;
            _localPlayers.RemoveAt(playerIndex);
            OnUserLeft?.Invoke(playerIndex);
            Debug.Log($"Removed User: {user.DisplayName.Value} - {user.ID.Value} from slot {playerIndex + 1}/{PlayerCount}");
        }
        
        private void OnUserChangedStatus(PlayerStatus status)
        {
            int readyCount = _localPlayers.Count(player => player.UserStatus.Value == PlayerStatus.Ready);
            OnUserReadyChange?.Invoke(readyCount);
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Lobby : ");
            sb.AppendLine(LobbyName.Value);
            sb.Append("ID: ");
            sb.AppendLine(LobbyID.Value);
            sb.Append("Code: ");
            sb.AppendLine(LobbyCode.Value);
            sb.Append("Locked: ");
            sb.AppendLine(Locked.Value.ToString());
            sb.Append("Private: ");
            sb.AppendLine(Private.Value.ToString());
            sb.Append("AvailableSlots: ");
            sb.AppendLine(AvailableSlots.Value.ToString());
            sb.Append("Max Players: ");
            sb.AppendLine(MaxPlayerCount.Value.ToString());
            sb.Append("LocalLobbyState: ");
            sb.AppendLine(LocalLobbyState.Value.ToString());
            sb.Append("Lobby LocalLobbyState Last Edit: ");
            sb.AppendLine(new DateTime(LastUpdated.Value).ToString(CultureInfo.InvariantCulture));
            // sb.Append("LocalLobbyColor: ");
            // sb.AppendLine(LocalLobbyColor.Value.ToString());
            sb.Append("RelayCode: ");
            sb.AppendLine(RelayCode.Value);

            return sb.ToString();
        }
    }
}