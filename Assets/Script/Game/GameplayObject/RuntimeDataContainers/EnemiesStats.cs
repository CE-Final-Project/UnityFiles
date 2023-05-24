using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Script.Game.GameplayObject.Character;
using UnityEngine;

namespace Script.Game.GameplayObject.RuntimeDataContainers
{
    public class EnemyStats
    {
        public int AttackDamage { get; private set; }
        public int Health { get; private set; }
        public int Population { get; private set; }
        
        public EnemyStats(int attackDamage, int health, int population)
        {
            AttackDamage = attackDamage;
            Health = health;
            Population = population;
        }
        
        public void AddPopulation()
        {
            Population++;
        }
        
        public void RemovePopulation()
        {
            Population--;
        }
        
        public void SetHealth(int health)
        {
            Health = health;
        }
        
        public void SetAttackDamage(int attackDamage)
        {
            AttackDamage = attackDamage;
        }
    }
    
    public class EnemiesStats : MonoBehaviour
    {
        
        private readonly Dictionary<string, EnemyStats> _enemiesStats = new Dictionary<string, EnemyStats>();
        
        private DateTime _startTime;
        private DateTime _playTime;
        
        private Coroutine _writeDataToCsvCoroutine;
        private StreamWriter _writer;
        
        private const int WriteDataToCsvInterval = 10; // Seconds

        public void StartTracking()
        {
            StopTracking();
            _startTime = DateTime.Now;
            string filePath = Application.persistentDataPath + $"/enemies_stats{_startTime:yyyy-M-d-HH-mm-ss}.csv";
            StartWriteDataToCsvCoroutine(filePath);
        }
        
        private void StartWriteDataToCsvCoroutine(string filePath)
        {
            bool fileExists = File.Exists(filePath);
            
            _writer = new StreamWriter(filePath, true);
            
            if (!fileExists)
            {
                _writer.WriteLine("Play Time (Second),EnemyType,AttackDamage,Health,Population");
            }
            
            _writeDataToCsvCoroutine = StartCoroutine(WriteDataToCsvCoroutine(filePath));
        }
        
        private IEnumerator WriteDataToCsvCoroutine(string filePath)
        {
            while (true)
            {
                // Wait before writing data again
                yield return new WaitForSecondsRealtime(WriteDataToCsvInterval);
                
                string playTime = DateTime.Now.Subtract(_startTime).ToString(@"hh\:mm\:ss");

                // Write data for each enemy
                foreach (var enemyStats in _enemiesStats)
                {
                    _writer.WriteLine($"{playTime},{enemyStats.Key},{enemyStats.Value.AttackDamage},{enemyStats.Value.Health},{enemyStats.Value.Population}");
                }
                _writer.Flush();
                Debug.Log($"Time: {playTime} - Enemy Data written to {filePath}");
            }
        }
        
        public void StopTracking()
        {
            if (_writeDataToCsvCoroutine != null)
            {
                StopCoroutine(_writeDataToCsvCoroutine);
                _writeDataToCsvCoroutine = null;
            }
            _writer?.Close();
            _enemiesStats.Clear();
        }
        
        public string GetStringEnemiesStats()
        {
            string enemiesStats = "";
            foreach (var enemyStats in _enemiesStats)
            {
                enemiesStats += $"{enemyStats.Key} - Attack Damage: {enemyStats.Value.AttackDamage} - Health: {enemyStats.Value.Health} - Population: {enemyStats.Value.Population}\n";
            }
            return enemiesStats;
        }
        
        public Dictionary<string, EnemyStats> GetEnemiesStatsMap()
        {
            return _enemiesStats;
        }
        
        [CanBeNull]
        public EnemyStats GetEnemyStats(string enemyType)
        {
            if (_enemiesStats.TryGetValue(enemyType, out EnemyStats stats))
            {
                return stats;
            }
            return null;
        }

        public void AddEnemy(ServerCharacter enemyServerCharacter)
        {
            if (_enemiesStats.TryGetValue(enemyServerCharacter.CharacterType.ToString(), out EnemyStats enemyStats))
            {
                if (enemyStats.Health != enemyServerCharacter.HitPoints)
                {
                    enemyStats.SetHealth(enemyServerCharacter.HitPoints);
                }
                
                if (enemyStats.AttackDamage != enemyServerCharacter.CharacterClass.Skill1.Config.Amount)
                {
                    enemyStats.SetAttackDamage(enemyServerCharacter.CharacterClass.Skill1.Config.Amount);
                }
                
                enemyStats.AddPopulation();
            }
            else
            {
                _enemiesStats.Add(enemyServerCharacter.CharacterType.ToString(),
                    new EnemyStats(enemyServerCharacter.CharacterClass.Skill1.Config.Amount,
                        enemyServerCharacter.HitPoints,
                        1));
            }
        }
        
        public void RemoveEnemy(ServerCharacter enemyServerCharacter)
        {
            if (_enemiesStats.TryGetValue(enemyServerCharacter.CharacterType.ToString(), out EnemyStats enemyStats))
            {
                enemyStats.RemovePopulation();
            }
        }
        
        public void SetAttack(string enemyType, int attackDamage)
        {
            if (_enemiesStats.TryGetValue(enemyType, out EnemyStats enemyStats))
            {
                enemyStats.SetAttackDamage(attackDamage);
            }
        }
        
        public void SetHealth(string enemyType, int health)
        {
            if (_enemiesStats.TryGetValue(enemyType, out EnemyStats enemyStats))
            {
                enemyStats.SetHealth(health);
            }
        }
    }
}