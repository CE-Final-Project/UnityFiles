using System;
using System.Collections.Generic;
using Script.Infrastructure;
using Script.Lobby;

namespace Script
{
    public enum LobbyQueryState
    {
        Empty,
        Fetching,
        Error,
        Fetched
    }
    
    public class LocalLobbyList
    {
        public CallbackValue<LobbyQueryState> QueryState = new CallbackValue<LobbyQueryState>();
        
        public Action<Dictionary<string, LocalLobby>> OnLobbyListChange;
        private Dictionary<string, LocalLobby> _currentLobbies = new Dictionary<string, LocalLobby>();
        
        public Dictionary<string, LocalLobby> CurrentLobbies
        {
            get => _currentLobbies;
            set
            {
                _currentLobbies = value;
                OnLobbyListChange?.Invoke(_currentLobbies);
            }
        }
    }
}