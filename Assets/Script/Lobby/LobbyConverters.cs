using System;
using System.Collections.Generic;
using System.Linq;
using Script.Game;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Script.Lobby
{
    public static class LobbyConverters
    {
        private const string RelayCodeKey = nameof(LocalLobby.RelayCode);
        private const string LobbyStateKey = nameof(LocalLobby.LocalLobbyState);
        private const string LastEditKey = nameof(LocalLobby.LastUpdated);
        
        private const string DisplayNameKey = nameof(LocalPlayer.DisplayName);
        private const string UserStatusKey = nameof(LocalPlayer.UserStatus);
        private const string CharacterKey = nameof(LocalPlayer.Character);
        
        
        public static Dictionary<string, string> LocalToRemoteLobbyData(LocalLobby lobby)
        {
            var data = new Dictionary<string, string>
            {
                { RelayCodeKey, lobby.RelayCode.Value },
                { LobbyStateKey, ((int)lobby.LocalLobbyState.Value).ToString() },
                { LastEditKey, lobby.LastUpdated.Value.ToString() }
            };
            return data;
        }
        
        public static Dictionary<string, string> LocalToRemoteUserData(LocalPlayer user)
        {
            var data = new Dictionary<string, string>();
            if (user == null || string.IsNullOrEmpty(user.ID.Value))
                return data;
            data.Add(DisplayNameKey, user.DisplayName.Value);
            data.Add(UserStatusKey, ((int)user.UserStatus.Value).ToString());
            data.Add(CharacterKey, ((int)user.Character.Value).ToString());
            return data;
        }
        
        public static void RemoteToLocal(Unity.Services.Lobbies.Models.Lobby remoteLobby, LocalLobby localLobby)
        {
            if (remoteLobby == null)
            {
                Debug.LogError("Remote lobby is null, cannot convert.");
                return;
            }
            
            if (localLobby == null)
            {
                Debug.LogError("Local Lobby is null, cannot convert");
                return;
            }
            
            localLobby.LobbyID.Value = remoteLobby.Id;
            localLobby.HostID.Value = remoteLobby.HostId;
            localLobby.LobbyName.Value = remoteLobby.Name;
            localLobby.LobbyCode.Value = remoteLobby.LobbyCode;
            localLobby.Private.Value = remoteLobby.IsPrivate;
            localLobby.AvailableSlots.Value = remoteLobby.AvailableSlots;
            localLobby.MaxPlayerCount.Value = remoteLobby.MaxPlayers;
            localLobby.LastUpdated.Value = remoteLobby.LastUpdated.ToFileTimeUtc();
            
            //Custom Lobby Data Conversions
            if (remoteLobby.Data != null)
            {
                if (remoteLobby.Data.ContainsKey(RelayCodeKey))
                {
                    localLobby.RelayCode.Value = remoteLobby.Data[RelayCodeKey].Value;
                }

                if (remoteLobby.Data.ContainsKey(LobbyStateKey))
                {
                    localLobby.LocalLobbyState.Value = Enum.Parse<LobbyState>(remoteLobby.Data[LobbyStateKey].Value);
                }
            }
            
            int index = 0;
            // var remotePlayerIDs = new List<string>();
            foreach (Player player in remoteLobby.Players)
            {
                string id = player.Id;
                // remotePlayerIDs.Add(id);
                bool isHost = remoteLobby.HostId.Equals(player.Id);
                string displayName = player.Data?.ContainsKey(DisplayNameKey) == true
                    ? player.Data[DisplayNameKey].Value
                    : default;
                CharacterType character = player.Data?.ContainsKey(CharacterKey) == true
                    ? Enum.Parse<CharacterType>(player.Data[CharacterKey].Value)
                    : CharacterType.None;
                PlayerStatus userStatus = player.Data?.ContainsKey(UserStatusKey) == true
                    ? Enum.Parse<PlayerStatus>(player.Data[UserStatusKey].Value)
                    : PlayerStatus.None;

                LocalPlayer localPlayer = localLobby.GetLocalPlayer(index);

                if (localPlayer == null)
                {
                    localPlayer = new LocalPlayer(id, index, isHost, displayName, character, userStatus);
                    localLobby.AddPlayer(index, localPlayer);
                }
                else
                {
                    localPlayer.ID.Value = id;
                    localPlayer.Index.Value = index;
                    localPlayer.IsHost.Value = isHost;
                    localPlayer.DisplayName.Value = displayName;
                    localPlayer.Character.Value = character;
                    localPlayer.UserStatus.Value = userStatus;
                }
                index++;
            }
        }

        public static List<LocalLobby> QueryToLocalLobbyList(QueryResponse response)
        {
            return response.Results.Select(RemoteToNewLocal).ToList();
        }

        private static LocalLobby RemoteToNewLocal(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            LocalLobby data = new LocalLobby();
            RemoteToLocal(lobby, data);
            return data;
        }
    }
}