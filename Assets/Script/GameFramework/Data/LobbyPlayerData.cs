using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

namespace Script.GameFramework.Models
{
    public class LobbyPlayerData
    {
        private string _id;
        private string _gameTag;
        private bool _isReady;
        public string Id => _id;
        public string GameTag => _gameTag;

        public bool IsReady
        {
            get => _isReady;
            set => _isReady = value;
        }

        public void Initialize(string id, string gameTag)
        {
            _id = id;
            _gameTag = gameTag;
        }
        
        public void Initialize(Dictionary<string,PlayerDataObject> playerData)
        {
            UpdateState(playerData);
        }

        public void UpdateState(Dictionary<string, PlayerDataObject> playerData)
        {
            if (playerData.ContainsKey("id"))
            {
                _id = playerData["id"].Value;
            }
            
            if (playerData.ContainsKey("gameTag"))
            {
                _gameTag = playerData["gameTag"].Value;
            }
            
            if (playerData.ContainsKey("isReady"))
            {
                _isReady = bool.Parse(playerData["isReady"].Value);
            }
        }
        
        public Dictionary<string, string> Serialize()
        {
            return new Dictionary<string, string>
            {
                {"id", _id},
                {"gameTag", _gameTag},
                {"isReady", _isReady.ToString()}
            };
        }
    }
}