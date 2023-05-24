using System;
using Script.Game.GameplayObject.Character;
using Script.Infrastructure;

namespace Script
{
    [Flags]
    public enum PlayerStatus
    {
        None = 0,
        Connecting = 1, // User has joined a lobby but has not yet connected to Relay.
        Lobby = 2, // User is in a lobby and connected to Relay.
        Ready = 4, // User has selected the ready button, to ready for the "game" to start.
        InGame = 8, // User is part of a "game" that has started.
        Menu = 16 // User is not in a lobby, in one of the main menus.
    }

    public class LocalPlayer
    {
        public CallbackValue<bool> IsHost = new CallbackValue<bool>(false);
        public CallbackValue<string> DisplayName = new CallbackValue<string>("");
        public CallbackValue<CharacterTypeEnum> Character = new CallbackValue<CharacterTypeEnum>(CharacterTypeEnum.None);
        public CallbackValue<PlayerStatus> UserStatus = new CallbackValue<PlayerStatus>(Script.PlayerStatus.None);
        public CallbackValue<string> ID = new CallbackValue<string>("");
        public CallbackValue<int> Index = new CallbackValue<int>(0);
        
        public DateTime LastUpdated;

        public LocalPlayer(string id, int index, bool isHost, string displayName = default,
            CharacterTypeEnum character = default, PlayerStatus status = default)
        {
            ID.Value = id;
            IsHost.Value = isHost;
            Index.Value = index;
            DisplayName.Value = displayName;
            Character.Value = character;
            UserStatus.Value = status;
        }
        
        public void ResetState()
        {
            IsHost.Value = false;
            Character.Value = CharacterTypeEnum.None;
            UserStatus.Value = Script.PlayerStatus.Menu;
        }
    }
}