using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Script.ConnectionManagement;
using Script.Game;
using Script.Game.GameplayObject;
using Script.Game.GameplayObject.Character;
using Script.Game.Messages;
using Script.Infrastructure.PubSub;
using Script.Utils;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using Random = UnityEngine.Random;

namespace Script.GameState
{
    [RequireComponent(typeof(NetcodeHooks))]
    public class ServerInGameState : GameStateBehaviour
    {
        
        // [SerializeField] private PersistentGameState persistentGameState;
        
        [SerializeField] private NetcodeHooks netCodeHooks;
        
        [SerializeField] private NetworkObject playerPrefab;
        
        [SerializeField] private Transform[] playerSpawnPoints;

        private List<Transform> _playerSpawnPointList = null;

        protected override GameState ActiveState => GameState.InGame;
        
        // Wait time constants for switching to post game after the game is won or lost
        private const float WinDelay = 5f;
        private const float LossDelay = 2.5f;
        
        public bool InitialSpawnDone { get; private set; }

        [Inject] private ISubscriber<LifeStateChangedEventMessage> _lifeStateChangedSubscriber;
        
        [Inject] private ConnectionManager _connectionManager;
        [Inject] private PersistantGameState _persistentGameState;

        protected override void Awake()
        {
            base.Awake();
            netCodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            netCodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
        }
        
        private void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
                return;
            }
            _persistentGameState.Reset();
            _lifeStateChangedSubscriber.Subscribe(OnLifeStateChangedEventMessage);
            
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;
            
            SessionManager<SessionPlayerData>.Instance.OnSessionStarted();
        }
        
        private void OnNetworkDespawn()
        {
            _lifeStateChangedSubscriber?.Unsubscribe(OnLifeStateChangedEventMessage);
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete -= OnSynchronizeComplete;
        }
        
        protected override void OnDestroy()
        {
            _lifeStateChangedSubscriber?.Unsubscribe(OnLifeStateChangedEventMessage);

            if (netCodeHooks)
            {
                netCodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
                netCodeHooks.OnNetworkDespawnHook -= OnNetworkDespawn;
            }
            
            base.OnDestroy();
        }

        private void OnSynchronizeComplete(ulong clientId)
        {
            if (InitialSpawnDone && PlayerServerCharacter.GetPlayerServerCharacter(clientId) is null){
                // Somebody joined after the initial spawn. This is a Late Join scenario. This player may have issues
                // either because multiple people are late-joining at once, or because some dynamic entities are
                //getting spawned while joining. But that's not something we can fully address ny changes in 
                // ServerInGameState.
                SpawnPlayer(clientId, true);
            }
        }

        private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            if (!InitialSpawnDone && loadSceneMode == LoadSceneMode.Single)
            {
                InitialSpawnDone = true;
                foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
                {
                    SpawnPlayer(kvp.Key, false);
                }
            }
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                StartCoroutine(WaitToCheckForGameOver());
            }
        }
        
        private IEnumerator WaitToCheckForGameOver()
        {
            // Wait until next frame so that the client's player character has despawned
            yield return null;
            CheckForGameOver();
        }

        private void SpawnPlayer(ulong clientId, bool lateJoin)
        {
            Transform spawnPoint = null;
            
            if (_playerSpawnPointList == null || _playerSpawnPointList.Count == 0)
            {
                _playerSpawnPointList = new List<Transform>(playerSpawnPoints);
            }
            
            Debug.Assert(_playerSpawnPointList.Count > 0, "PlayerSpawnPoints array should have at least 1 spawn points.");

            int index = Random.Range(0, _playerSpawnPointList.Count);
            spawnPoint = _playerSpawnPointList[index];
            _playerSpawnPointList.RemoveAt(index);

            NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            
            NetworkObject newPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

            ServerCharacter newPlayerCharacter = newPlayer.GetComponent<ServerCharacter>();

            if (spawnPoint != null)
            {
                Transform transform1 = newPlayerCharacter.transform;
                transform1.position = spawnPoint.position;
                transform1.rotation = spawnPoint.rotation;
            }

            newPlayerCharacter.PersistentState = _persistentGameState;
            
            bool persistentPlayerExists = playerNetworkObject.TryGetComponent(out PersistentPlayer persistentPlayer);
            Assert.IsTrue(persistentPlayerExists, $"Matching persistent PersistentPlayer for client {clientId} not found!");
            
            bool networkAvatarGuidStateExists = newPlayer.TryGetComponent(out NetworkAvatarGuidState networkAvatarGuidState);
            Assert.IsTrue(networkAvatarGuidStateExists, "NetworkCharacterGuidState not found on player avatar!");

            if (lateJoin)
            {
                var sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);

                if (sessionPlayerData is { HasCharacterSpawned: true })
                {
                    Transform transform1 = newPlayer.transform;
                    transform1.position = new Vector3(sessionPlayerData.Value.PlayerPosition.x, sessionPlayerData.Value.PlayerPosition.y, 0);
                }
            }

            networkAvatarGuidState.avatarNetworkGuid.Value = 
                persistentPlayer.NetworkAvatarGuidState.avatarNetworkGuid.Value;
            
            if (newPlayer.TryGetComponent(out NetworkNameState networkNameState))
            {
                networkNameState.Name.Value = persistentPlayer.NetworkNameState.Name.Value;
            }
            
            // Add player stats to the persistent game state
            _persistentGameState.AddPlayerStats(newPlayerCharacter.CharacterType.ToString());

            newPlayer.SpawnWithOwnership(clientId, true);
        }
        
        private void OnLifeStateChangedEventMessage(LifeStateChangedEventMessage message)
        {
            switch (message.CharacterType)
            {
                case CharacterTypeEnum.Elf:
                case CharacterTypeEnum.Knight:
                case CharacterTypeEnum.Lizard:
                case CharacterTypeEnum.Wizard:
                    // Every time a player's life state changes to fainted we check to see if game is over
                    if (message.NewLifeState == LifeState.Dead)
                    {
                        CheckForGameOver();
                    }

                    break;
                // case CharacterTypeEnum.ImpBoss:
                //     if (message.NewLifeState == LifeState.Dead)
                //     {
                //         BossDefeated();
                //     }
                //     break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void CheckForGameOver()
        {
            // Check the life state of all players in the scene
            foreach (var serverCharacter in PlayerServerCharacter.GetPlayerServerCharacters())
            {
                // if any player is alive just return
                if (serverCharacter && serverCharacter.LifeState == LifeState.Alive)
                {
                    return;
                }
            }

            // If we made it this far, all players are down! switch to post game
            StartCoroutine(CoroGameOver(LossDelay, false));
        }
        
        private IEnumerator CoroGameOver(float delay, bool victory)
        {
            yield return new WaitForSeconds(delay);
            
            SceneLoaderWrapper.Instance.LoadScene("PostGame", true);
        }
    }
}