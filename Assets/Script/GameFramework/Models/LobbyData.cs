namespace Script.GameFramework.Models
{
    public class LobbyData
    {
        public string LobbyId { get; set; } = string.Empty;
        public string LobbyName { get; set; } = "Untitled Lobby";
        public string LobbyCode { get; set; } = string.Empty;
        public bool IsPrivate { get; set; } = false;
        public string HostId { get; set; } = string.Empty;
        public int MaxPlayer { get; set; } = 4;
    }
}