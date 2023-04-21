using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace Script.Game.GameplayObject.RuntimeDataContainers
{
    public class PlayerStats
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


    public class PlayersStats : MonoBehaviour
    {
        private Dictionary<ulong, PlayerStats> _playerStatsMap = new Dictionary<ulong, PlayerStats>();
        private DateTime _startTime;
        private DateTime _playTime;
        
        private Coroutine _writeDataToCsvCoroutine;
        private StreamWriter _writer;
        
        private const int WriteDataToCsvInterval = 5; // Seconds

        public void StartTracking()
        {
            StopTracking(); // Stop tracking if it's already running or if the game is restarted
            _startTime = DateTime.Now;
            string filePath = Application.persistentDataPath + $"/players_stats{_startTime:yyyy-M-d-HH-mm-ss}.csv";
            StartWriteDataToCsvCoroutine(filePath);
        }

        private void StartWriteDataToCsvCoroutine(string filePath)
        {
            bool fileExists = File.Exists(filePath);
            
            _writer = new StreamWriter(filePath, true);
            
            if (!fileExists)
            {
                _writer.WriteLine("Play Time (Second),PlayerId,CharacterType,KillCount,DeathCount,DamageDealt,DamageTaken,HealingDone,HealingTaken");
            }
            
            _writeDataToCsvCoroutine = StartCoroutine(WriteDataToCsvCoroutine(filePath));
        }

        public void StopTracking()
        {
            if (_writeDataToCsvCoroutine != null)
            {
                StopCoroutine(_writeDataToCsvCoroutine);
            }
            _writer?.Close();
            _playerStatsMap.Clear();   
        }
        
        private IEnumerator WriteDataToCsvCoroutine(string filePath)
        {
            while (true)
            {
                // Wait before writing data again
                yield return new WaitForSecondsRealtime(WriteDataToCsvInterval);
                
                string playTime = DateTime.Now.Subtract(_startTime).ToString(@"hh\:mm\:ss");

                // Write data for each player
                foreach (var playerStats in _playerStatsMap)
                {
                    string row = $"{playTime}, {playerStats.Key},{playerStats.Value.CharacterType},{playerStats.Value.KillCount},{playerStats.Value.DeathCount},{playerStats.Value.DamageDealt},{playerStats.Value.DamageTaken},{playerStats.Value.HealingDone},{playerStats.Value.HealingTaken}";
                    _writer.WriteLine(row);
                }
                _writer.Flush();
                Debug.Log($"Time: {playTime} - Player Data written to {filePath}");
            }
        }
        
        public double GetCurrentPlayTime()
        {
            double playTime = DateTime.Now.Subtract(_startTime).TotalSeconds;
            return playTime;
        }

        public void AddPlayer(ulong clientId, string characterType)
        {
            if (_playerStatsMap.ContainsKey(clientId))
            {
                Debug.LogError($"Player {clientId} with character type {characterType} already exists");
            }
            
            _playerStatsMap.Add(clientId, new PlayerStats(characterType, 0, 0, 0, 0, 0, 0));
        }
        
        public void AddKill(ulong clientId)
        {
            if (!_playerStatsMap.ContainsKey(clientId))
            {
                Debug.LogError($"Player {clientId} does not exist");
            }
            
            _playerStatsMap[clientId].AddKill();
        }
        
        public void AddDeath(ulong clientId)
        {
            if (!_playerStatsMap.ContainsKey(clientId))
            {
                Debug.LogError($"Player {clientId} does not exist");
            }
            
            _playerStatsMap[clientId].AddDeath();
        }
        
        public void AddDamageDealt(ulong clientId, int damage)
        {
            if (!_playerStatsMap.ContainsKey(clientId))
            {
                Debug.LogError($"Player {clientId} does not exist");
            }
            _playerStatsMap[clientId].AddDamageDealt(damage);
        }
        
        public void AddDamageTaken(ulong clientId, int damage)
        {
            if (!_playerStatsMap.ContainsKey(clientId))
            {
                Debug.LogError($"Player {clientId} does not exist");
            }
            _playerStatsMap[clientId].AddDamageTaken(damage);
        }
        
        public void AddHealingDone(ulong clientId, int healing)
        {
            if (!_playerStatsMap.ContainsKey(clientId))
            {
                Debug.LogError($"Player {clientId} does not exist");
            }
            
            _playerStatsMap[clientId].AddHealingDone(healing);
        }
        
        public void AddHealingTaken(ulong clientId, int healing)
        {
            if (!_playerStatsMap.ContainsKey(clientId))
            {
                Debug.LogError($"Player {clientId} does not exist");
            }
            
            _playerStatsMap[clientId].AddHealingTaken(healing);
        }
        
        public void RemovePlayer(ulong clientId)
        {
            if (!_playerStatsMap.ContainsKey(clientId))
            {
                Debug.LogError($"Player {clientId} does not exist");
            }
            
            _playerStatsMap.Remove(clientId);
        }

        public string GetStringPlayersStats()
        {
            string stats = "";
            foreach (var playerStats in _playerStatsMap)
            {
                stats += $"Player {playerStats.Key}\n" +
                         $"Character type: {playerStats.Value.CharacterType}\n" +
                         $"Kill count: {playerStats.Value.KillCount}\n" +
                         $"Death Count: {playerStats.Value.DeathCount}\n" +
                         $"Damage Dealt: {playerStats.Value.DamageDealt}\n" +
                         $"Damage Taken: {playerStats.Value.DamageTaken}\n" +
                         $"Healing Done: {playerStats.Value.HealingDone}\n" +
                         $"Healing Taken: {playerStats.Value.HealingTaken}\n\n";
            }

            return stats;
        }
        
        public Dictionary<ulong, PlayerStats> GetPlayerStatsMap()
        {
            return _playerStatsMap;
        }
        
        [CanBeNull]
        public PlayerStats GetPlayerStats(ulong clientId)
        {
            if (_playerStatsMap.TryGetValue(clientId, out PlayerStats playerStats))
            {
                return playerStats;
            }
            
            return null;
        }
    }
}