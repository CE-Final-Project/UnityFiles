using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

namespace Script.GameFramework.Data
{
    public class LobbyPlayerData
    {
        private string _id;
        private string _name;
        private string _characterId;
        private string _gameTag;
        private bool _isReady;
        public string Id => _id;
        public string Name => _name;
        public string CharacterId => _characterId;

        public string GameTag => _gameTag;

        public bool IsReady
        {
            get => _isReady;
            set => _isReady = value;
        }

        public void Initialize(string id, string name, string charecterId, string gameTag)
        {
            _id = id;
            _name = name;
            _characterId = charecterId;
            _gameTag = gameTag;
        }
        
        public void Initialize(Dictionary<string,PlayerDataObject> playerData)
        {
            UpdateState(playerData);
        }

        private void UpdateState(IReadOnlyDictionary<string, PlayerDataObject> playerData)
        {
            if (playerData.ContainsKey("id"))
            {
                _id = playerData["id"].Value;
            }
            
            if (playerData.ContainsKey("name"))
            {
                _name = playerData["name"].Value;
            }
            
            if (playerData.ContainsKey("characterId"))
            {
                _characterId = playerData["characterId"].Value;
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
                {"name", _name},
                {"characterId", _characterId},
                {"gameTag", _gameTag},
                {"isReady", _isReady.ToString()}
            };
        }
    }
}