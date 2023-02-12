namespace Script.GameFramework.Data
{
    public class LobbyCreatedEventArgs
    {
        public string LobbyId { get; set; }
        public string LobbyCode { get; set; }
        public bool IsPrivate { get; set; }
        public int MaxPlayer { get; set; }
        public string HostId { get; set; }
    }
}