using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

namespace Script.Lobby
{
    /// <summary>
    /// Data for a local lobby user instance. This will update data and is observed to know when to push local user changes to the entire lobby.
    /// </summary>
    [Serializable]
    public class LocalLobbyUser
    {
        public event Action<LocalLobbyUser> Changed;

        public LocalLobbyUser()
        {
            _userData = new UserData(isHost: false, displayName: null, id: null);
        }

        public struct UserData
        {
            public bool IsHost { get; set; }
            public string DisplayName { get; set; }
            public string ID { get; set; }

            public UserData(bool isHost, string displayName, string id)
            {
                IsHost = isHost;
                DisplayName = displayName;
                ID = id;
            }
        }

        private UserData _userData;

        public void ResetState()
        {
            _userData = new UserData(false, _userData.DisplayName, _userData.ID);
        }

        /// <summary>
        /// Used for limiting costly OnChanged actions to just the members which actually changed.
        /// </summary>
        [Flags]
        public enum UserMembers
        {
            IsHost = 1,
            DisplayName = 2,
            ID = 4,
        }

        private UserMembers _lastChanged;

        public bool IsHost
        {
            get => _userData.IsHost;
            set
            {
                if (_userData.IsHost != value)
                {
                    _userData.IsHost = value;
                    _lastChanged = UserMembers.IsHost;
                    OnChanged();
                }
            }
        }

        public string DisplayName
        {
            get => _userData.DisplayName;
            set
            {
                if (_userData.DisplayName != value)
                {
                    _userData.DisplayName = value;
                    _lastChanged = UserMembers.DisplayName;
                    OnChanged();
                }
            }
        }

        public string ID
        {
            get => _userData.ID;
            set
            {
                if (_userData.ID != value)
                {
                    _userData.ID = value;
                    _lastChanged = UserMembers.ID;
                    OnChanged();
                }
            }
        }


        public void CopyDataFrom(LocalLobbyUser lobby)
        {
            UserData data = lobby._userData;
            int lastChanged = // Set flags just for the members that will be changed.
                (_userData.IsHost == data.IsHost ? 0 : (int)UserMembers.IsHost) |
                (_userData.DisplayName == data.DisplayName ? 0 : (int)UserMembers.DisplayName) |
                (_userData.ID == data.ID ? 0 : (int)UserMembers.ID);

            if (lastChanged == 0) // Ensure something actually changed.
            {
                return;
            }

            _userData = data;
            _lastChanged = (UserMembers)lastChanged;

            OnChanged();
        }

        private void OnChanged()
        {
            Changed?.Invoke(this);
        }

        public Dictionary<string, PlayerDataObject> GetDataForUnityServices() =>
            new Dictionary<string, PlayerDataObject>()
            {
                {"DisplayName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, DisplayName)},
            };
    }
}