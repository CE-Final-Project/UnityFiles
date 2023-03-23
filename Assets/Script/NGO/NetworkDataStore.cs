using System;
using System.Collections.Generic;
using System.Linq;
using Script.Game.GameplayObject.Character;
using Unity.Netcode;
using UnityEngine.Events;

namespace Script.NGO
{
    public class NetworkDataStore : NetworkBehaviour
    {
        public static NetworkDataStore Instance;
        
        private Dictionary<ulong, PlayerData> _playerData = new Dictionary<ulong, PlayerData>();
        private ulong _localPlayerId;
        
        private Action<PlayerData> _onGetCurrentCallback;
        private UnityEvent<PlayerData> _onEachPlayerCallback;

        public void Awake()
        {
            Instance = this;
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            if (Instance == this)
                Instance = null;
        }
        
        public override void OnNetworkSpawn()
        {
            _localPlayerId = NetworkManager.Singleton.LocalClientId;
        }
        
        public void AddPlayer(ulong id, string playerName, float health, CharacterTypeEnum characterTypeEnum, int score = 0)
        {
            if (!IsServer)
                return;

            if (!_playerData.ContainsKey(id))
                _playerData.Add(id, new PlayerData(playerName, id, health, characterTypeEnum, score));
            else
                _playerData[id] = new PlayerData(playerName, id, health, characterTypeEnum, score);
        }
        
        public int UpdateScore(ulong id, int delta)
        {
            if (!IsServer)
                return int.MinValue;

            if (_playerData.ContainsKey(id))
            {
                _playerData[id].Score += delta;
                return _playerData[id].Score;
            }

            return int.MinValue;
        }
        
        public float UpdateHealth(ulong id, float delta)
        {
            if (!IsServer)
                return float.MinValue;

            if (_playerData.ContainsKey(id))
            {
                _playerData[id].Health += delta;
                return _playerData[id].Health;
            }

            return float.MinValue;
        }
        
        public void GetAllPlayerData(UnityEvent<PlayerData> onEachPlayer)
        {
            _onEachPlayerCallback = onEachPlayer;
            GetAllPlayerData_ServerRpc(_localPlayerId);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void GetAllPlayerData_ServerRpc(ulong callerId)
        {
            var sortedData = _playerData.Select(kvp => kvp.Value).OrderByDescending(data => data.Score);
            GetAllPlayerData_ClientRpc(callerId, sortedData.ToArray());
        }
        
        [ClientRpc]
        private void GetAllPlayerData_ClientRpc(ulong callerId, PlayerData[] sortedData)
        {
            if (callerId != _localPlayerId)
                return;
            
            foreach (PlayerData playerData in sortedData)
            {
                _onEachPlayerCallback.Invoke(playerData);
            }

            _onEachPlayerCallback = null;
        }
        
        public void GetPlayerData(ulong id, Action<PlayerData> onGet)
        {
            _onGetCurrentCallback = onGet;
            GetPlayerData_ServerRpc(id, _localPlayerId);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void GetPlayerData_ServerRpc(ulong id, ulong callerId)
        {
            if (_playerData.ContainsKey(id))
            {
                GetPlayerData_ClientRpc(callerId, _playerData[id]);
            } else
            {
                GetPlayerData_ClientRpc(callerId, new PlayerData("Unknown", id, 0, CharacterTypeEnum.None, 0));
            }
        }
        
        [ClientRpc]
        private void GetPlayerData_ClientRpc(ulong callerId, PlayerData playerData)
        {
            if (callerId != _localPlayerId)
                return;
            
            _onGetCurrentCallback.Invoke(playerData);
            _onGetCurrentCallback = null;
        }
    }
}