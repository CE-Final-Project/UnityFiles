using System.Collections.Generic;

namespace Script.Lobby
{
    public struct LobbyListFetchedMessage
    {
        public readonly IReadOnlyList<LocalLobby> LocalLobbies;

        public LobbyListFetchedMessage(List<LocalLobby> localLobbies)
        {
            LocalLobbies = localLobbies;
        }
    }
}
