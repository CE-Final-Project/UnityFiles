using System.Collections.Generic;
using Script.Game.GameplayObject.Character;
using UnityEngine;

namespace Script.GameState
{

    public struct PlayerStats
    {
        public string CharacterType;
        public int KillCount;
        public int DeathCount;
        public int DamageDealt;
        public int DamageTaken;
        public int HealingDone;
        public int HealingTaken;
        
        public PlayerStats(string characterType)
        {
            CharacterType = characterType;
            KillCount = 0;
            DeathCount = 0;
            DamageDealt = 0;
            DamageTaken = 0;
            HealingDone = 0;
            HealingTaken = 0;
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
    
    public struct EnemyStats
    {
        public string EnemyType;
        public int Population;
        public int DeathCount;
        
        public EnemyStats(string enemyType)
        {
            EnemyType = enemyType;
            Population = 0;
            DeathCount = 0;
        }
        
        public void AddDeath()
        {
            DeathCount++;
        }
        
        public void AddPopulation()
        {
            Population++;
        }
        
        public void RemovePopulation()
        {
            Population--;
        }
    }
    
    public class PersistantGameState
    {
        public Dictionary<string, PlayerStats> PlayerStatsList = new Dictionary<string, PlayerStats>();
        public Dictionary<string, EnemyStats> EnemyStatsList = new Dictionary<string, EnemyStats>();
        
        public void AddPlayerStats(string characterType)
        {
            if (PlayerStatsList.ContainsKey(characterType))
            {
                return;
            }
            
            PlayerStatsList.Add(characterType, new PlayerStats(characterType));
        }
        
        public void AddEnemyStats(string enemyType)
        {
            if (EnemyStatsList.ContainsKey(enemyType))
            {
                return;
            }
            
            EnemyStatsList.Add(enemyType, new EnemyStats(enemyType));
        }
        
        public void Reset()
        {
            PlayerStatsList.Clear();
            EnemyStatsList.Clear();
        }
    }
}