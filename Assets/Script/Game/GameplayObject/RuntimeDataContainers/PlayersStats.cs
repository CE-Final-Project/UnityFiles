using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Script.Game.GameplayObject.RuntimeDataContainers
{
    
    public class PlayerId : IEquatable<PlayerId>
    {
        public ulong ClientId { get; }

        public PlayerId(ulong clientId)
        {
            ClientId = clientId;
        }

        public bool Equals(PlayerId other)
        {
            return other != null && ClientId == other.ClientId;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ClientId.GetHashCode();
        }
    }


    public class PlayersStats : MonoBehaviour
    {
        private struct PlayerStats
        {
            public string CharacterType { get; private set; }
            public int KillCount { get; private set; }
            public int DeathCount { get; private set; }
            public int DamageDealt { get; private set; }
            public int DamageTaken { get; private set; }
            public int HealingDone { get; private set; }
            public int HealingTaken { get; private set; }

            public PlayerStats(string characterType, int killCount, int deathCount, int damageDealt, int damageTaken, int healingDone, int healingTaken)
            {
                CharacterType = characterType;
                KillCount = killCount;
                DeathCount = deathCount;
                DamageDealt = damageDealt;
                DamageTaken = damageTaken;
                HealingDone = healingDone;
                HealingTaken = healingTaken;
            }
            
            public void AddKill()
            {
                KillCount++;
            }
            
            public void AddDeath()
            {
                DeathCount++;
            }
            
            public void AddDamageDealt(int damage)
            {
                DamageDealt += damage;
            }
            
            public void AddDamageTaken(int damage)
            {
                DamageTaken += damage;
            }
            
            public void AddHealingDone(int healing)
            {
                HealingDone += healing;
            }
            
            public void AddHealingTaken(int healing)
            {
                HealingTaken += healing;
            }
        }
        
         public static PlayersStats Instance { get; private set; }
         
         private Dictionary<PlayerId, PlayerStats> _playerStatsMap = new Dictionary<PlayerId, PlayerStats>();
            
        private void Awake()
        {
            if (Instance != null)
            {
                throw new Exception("There should be only one PlayersStats object in the scene");
            }
    
            ClearData();
            
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        
        public void ClearData()
        {
            _playerStatsMap.Clear();    
        }
        
        public void AddPlayer(ulong clientId, string characterType)
        {
            PlayerId playerId = new(clientId);
            if (_playerStatsMap.ContainsKey(playerId))
            {
                Debug.LogError($"Player {clientId} with character type {characterType} already exists");
            }
            
            _playerStatsMap.Add(playerId, new PlayerStats(characterType, 0, 0, 0, 0, 0, 0));
        }
        
        public void AddKill(ulong clientId)
        {
            PlayerId playerId = new(clientId);
            if (!_playerStatsMap.ContainsKey(playerId))
            {
                Debug.LogError($"Player {clientId} does not exist");
            }
            
            _playerStatsMap[playerId].AddKill();
        }
        
        public void AddDeath(ulong clientId)
        {
            PlayerId playerId = new(clientId);
            if (!_playerStatsMap.ContainsKey(playerId))
            {
                Debug.LogError($"Player {clientId} does not exist");
            }
            
            _playerStatsMap[playerId].AddDeath();
        }
        
        public void AddDamageDealt(ulong clientId, int damage)
        {
            PlayerId playerId = new(clientId);
            if (_playerStatsMap.TryGetValue(playerId, out PlayerStats playerStats))
            {
                playerStats.AddDamageDealt(damage);
            }
        }
        
        public void AddDamageTaken(ulong clientId, int damage)
        {
            PlayerId playerId = new(clientId);
            if (_playerStatsMap.TryGetValue(playerId, out PlayerStats playerStats))
            {
                playerStats.AddDamageTaken(damage);
            }
        }
        
        public void AddHealingDone(ulong clientId, int healing)
        {
            PlayerId playerId = new(clientId);
            if (!_playerStatsMap.ContainsKey(playerId))
            {
                Debug.LogError($"Player {clientId} does not exist");
            }
            
            _playerStatsMap[playerId].AddHealingDone(healing);
        }
        
        public void AddHealingTaken(ulong clientId, int healing)
        {
            PlayerId playerId = new(clientId);
            if (!_playerStatsMap.ContainsKey(playerId))
            {
                Debug.LogError($"Player {clientId} does not exist");
            }
            
            _playerStatsMap[playerId].AddHealingTaken(healing);
        }
        
        public void RemovePlayer(ulong clientId)
        {
            PlayerId playerId = new(clientId);
            if (!_playerStatsMap.ContainsKey(playerId))
            {
                Debug.LogError($"Player {clientId} does not exist");
            }
            
            _playerStatsMap.Remove(playerId);
        }

        public string GetPlayersStats()
        {
            string stats = "";
            foreach (var playerStats in _playerStatsMap)
            {
                stats += $"Player {playerStats.Key.ClientId}\n" +
                         $"Character type: {playerStats.Value.CharacterType}\n" +
                         $"Kill count: {playerStats.Value.KillCount}\n" +
                         $"Death Count: {playerStats.Value.DeathCount}\n" +
                         $"Damage Dealt: {playerStats.Value.DamageDealt}\n" +
                         $"Damage Taken: {playerStats.Value.DamageTaken}\n" +
                         $"Healing Done: {playerStats.Value.HealingDone}\n" +
                         $"Healing Taken: {playerStats.Value.HealingTaken}";
            }

            return stats;
        }
    }
}