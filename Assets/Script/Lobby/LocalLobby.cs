using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Script.Lobby
{
    /// <summary>
    /// A local wrapper around a lobby's remote data, with additional functionality for providing that data to UI elements and tracking local player objects.
    /// </summary>
    [Serializable]
    public sealed class LocalLobby
    {
        public event Action<LocalLobby> Changed;

        /// <summary>
        /// Create a list of new LocalLobbies from the result of a lobby list query.
        /// </summary>
        public static List<LocalLobby> CreateLocalLobbies(QueryResponse response)
        {
            var retLst = new List<LocalLobby>();
            foreach (var lobby in response.Results)
            {
                retLst.Add(Create(lobby));
            }
            return retLst;
        }

        public static LocalLobby Create(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            var data = new LocalLobby();
            data.ApplyRemoteData(lobby);
            return data;
        }

        private Dictionary<string, LocalLobbyUser> _lobbyUsers = new Dictionary<string, LocalLobbyUser>();
        public Dictionary<string, LocalLobbyUser> LobbyUsers => _lobbyUsers;

        public struct LobbyData
        {
            public string LobbyID { get; set; }
            public string LobbyCode { get; set; }
            public string RelayJoinCode { get; set; }
            public string RelayRegion { get; set; }
            public string LobbyName { get; set; }
            public bool Private { get; set; }
            public int MaxPlayerCount { get; set; }

            public LobbyData(LobbyData existing)
            {
                LobbyID = existing.LobbyID;
                LobbyCode = existing.LobbyCode;
                RelayJoinCode = existing.RelayJoinCode;
                RelayRegion = existing.RelayRegion;
                LobbyName = existing.LobbyName;
                Private = existing.Private;
                MaxPlayerCount = existing.MaxPlayerCount;
            }

            public LobbyData(string lobbyCode)
            {
                LobbyID = null;
                LobbyCode = lobbyCode;
                RelayJoinCode = null;
                RelayRegion = null;
                LobbyName = null;
                Private = false;
                MaxPlayerCount = -1;
            }
        }

        private LobbyData _data;
        public LobbyData Data => new LobbyData(_data);

        public void AddUser(LocalLobbyUser user)
        {
            if (!_lobbyUsers.ContainsKey(user.ID))
            {
                DoAddUser(user);
                OnChanged();
            }
        }

        private void DoAddUser(LocalLobbyUser user)
        {
            _lobbyUsers.Add(user.ID, user);
            user.Changed += OnChangedUser;
        }

        public void RemoveUser(LocalLobbyUser user)
        {
            DoRemoveUser(user);
            OnChanged();
        }

        private void DoRemoveUser(LocalLobbyUser user)
        {
            if (!_lobbyUsers.ContainsKey(user.ID))
            {
                Debug.LogWarning($"Player {user.DisplayName}({user.ID}) does not exist in lobby: {LobbyID}");
                return;
            }

            _lobbyUsers.Remove(user.ID);
            user.Changed -= OnChangedUser;
        }

        private void OnChangedUser(LocalLobbyUser user)
        {
            OnChanged();
        }

        private void OnChanged()
        {
            Changed?.Invoke(this);
        }

        public string LobbyID
        {
            get => _data.LobbyID;
            set
            {
                _data.LobbyID = value;
                OnChanged();
            }
        }

        public string LobbyCode
        {
            get => _data.LobbyCode;
            set
            {
                _data.LobbyCode = value;
                OnChanged();
            }
        }

        public string RelayJoinCode
        {
            get => _data.RelayJoinCode;
            set
            {
                _data.RelayJoinCode = value;
                OnChanged();
            }
        }
        
        public string RelayRegion
        {
            get => _data.RelayRegion;
            set
            {
                _data.RelayRegion = value;
                OnChanged();
            }
        }

        public string LobbyName
        {
            get => _data.LobbyName;
            set
            {
                _data.LobbyName = value;
                OnChanged();
            }
        }

        public bool Private
        {
            get => _data.Private;
            set
            {
                _data.Private = value;
                OnChanged();
            }
        }

        public int PlayerCount => _lobbyUsers.Count;

        public int MaxPlayerCount
        {
            get => _data.MaxPlayerCount;
            set
            {
                _data.MaxPlayerCount = value;
                OnChanged();
            }
        }

        public void CopyDataFrom(LobbyData data, Dictionary<string, LocalLobbyUser> currUsers)
        {
            _data = data;

            if (currUsers == null)
            {
                _lobbyUsers = new Dictionary<string, LocalLobbyUser>();
            }
            else
            {
                var toRemove = new List<LocalLobbyUser>();
                foreach (var oldUser in _lobbyUsers)
                {
                    if (currUsers.ContainsKey(oldUser.Key))
                    {
                        oldUser.Value.CopyDataFrom(currUsers[oldUser.Key]);
                    }
                    else
                    {
                        toRemove.Add(oldUser.Value);
                    }
                }

                foreach (LocalLobbyUser remove in toRemove)
                {
                    DoRemoveUser(remove);
                }

                foreach (var currUser in currUsers)
                {
                    if (!_lobbyUsers.ContainsKey(currUser.Key))
                    {
                        DoAddUser(currUser.Value);
                    }
                }
            }

            OnChanged();
        }

        public Dictionary<string, DataObject> GetDataForUnityServices() =>
            new Dictionary<string, DataObject>()
            {
                {"RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member,  RelayJoinCode)},
                {"RelayRegion", new DataObject(DataObject.VisibilityOptions.Member,  RelayRegion)}
            };

        public void ApplyRemoteData(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            var info = new LobbyData(); // Technically, this is largely redundant after the first assignment, but it won't do any harm to assign it again.
            info.LobbyID = lobby.Id;
            info.LobbyCode = lobby.LobbyCode;
            info.Private = lobby.IsPrivate;
            info.LobbyName = lobby.Name;
            info.MaxPlayerCount = lobby.MaxPlayers;

            if (lobby.Data != null)
            {
                info.RelayJoinCode = lobby.Data.ContainsKey("RelayJoinCode") ? lobby.Data["RelayJoinCode"].Value : null; // By providing RelayCode through the lobby data with Member visibility, we ensure a client is connected to the lobby before they could attempt a relay connection, preventing timing issues between them.
                info.RelayRegion = lobby.Data.ContainsKey("RelayRegion") ? lobby.Data["RelayRegion"].Value : null;
            }
            else
            {
                info.RelayJoinCode = null;
                info.RelayRegion = null;
            }

            var lobbyUsers = new Dictionary<string, LocalLobbyUser>();
            foreach (Player player in lobby.Players)
            {
                if (player.Data != null)
                {
                    if (LobbyUsers.ContainsKey(player.Id))
                    {
                        lobbyUsers.Add(player.Id, LobbyUsers[player.Id]);
                        continue;
                    }
                }

                // If the player isn't connected to Relay, get the most recent data that the lobby knows.
                // (If we haven't seen this player yet, a new local representation of the player will have already been added by the LocalLobby.)
                LocalLobbyUser incomingData = new LocalLobbyUser
                {
                    IsHost = lobby.HostId.Equals(player.Id),
                    DisplayName = player.Data?.ContainsKey("DisplayName") == true ? player.Data["DisplayName"].Value : default,
                    ID = player.Id
                };

                lobbyUsers.Add(incomingData.ID, incomingData);
            }

            CopyDataFrom(info, lobbyUsers);
        }

        public void Reset(LocalLobbyUser localUser)
        {
            CopyDataFrom(new LobbyData(), new Dictionary<string, LocalLobbyUser>());
            AddUser(localUser);
        }
    }
}
