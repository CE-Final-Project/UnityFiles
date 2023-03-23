using System;
using Unity.Netcode;
using UnityEngine;

namespace Script.NGO
{
    public class InGameRunner : NetworkBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        
        [SerializeField] private NetworkDataStore networkDataStore;
        
        public Action OnGameBeginning;
        private Action _onConnectionVerified;
        private Action _onGameEnd;
        
        private int _expectedPlayerCount;
        private bool? _canSpawnInGameObjects;
        private bool _hasConnected;
        
        private PlayerData _localPlayerData;
        
        private static InGameRunner _instance;

        public static InGameRunner Instance
        {
            get
            {
                if (_instance is null) return _instance = FindObjectOfType<InGameRunner>();
                return _instance;
            }
        }

        public void Initialize(Action onConnectionVerified, int expectedPlayerCount, Action onGameBegin, Action onGameEnd, LocalPlayer localUser)
        {
            _onConnectionVerified = onConnectionVerified;
            _expectedPlayerCount = expectedPlayerCount;
            OnGameBeginning = onGameBegin;
            _onGameEnd = onGameEnd;
            _localPlayerData = new PlayerData(localUser.DisplayName.Value, 0, 10, localUser.Character.Value);
        }
        
        public override void OnNetworkSpawn()
        {
            if (IsHost)
                FinishInitialize();
            _localPlayerData = new PlayerData(_localPlayerData.Name, NetworkManager.Singleton.LocalClientId, _localPlayerData.Health, _localPlayerData.CharacterTypeEnum);
            VerifyConnection_ServerRpc(_localPlayerData.Id);
        }

        public override void OnNetworkDespawn()
        {
            _onGameEnd();
        }
        
        private void FinishInitialize()
        {
           
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void VerifyConnection_ServerRpc(ulong clientId)
        {
            VerifyConnection_ClientRpc(clientId);
        }
        
        [ClientRpc]
        private void VerifyConnection_ClientRpc(ulong clientId)
        {
            if (clientId == _localPlayerData.Id)
                VerifyConnectionConfirm_ServerRpc(_localPlayerData);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void VerifyConnectionConfirm_ServerRpc(PlayerData clientData)
        {
            // TODO: Initialize player character and spawn it in the game
        }
    }
}