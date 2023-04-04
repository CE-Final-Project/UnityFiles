using System.Collections.Generic;
using Script.Game.GameplayObject.Character;

namespace Script.GameState
{

    public struct PlayerStats
    {
        public string CharacterType;
        public ulong KillCount;
        public ulong DeathCount;
        public ulong DamageDealt;
        public ulong DamageTaken;
        public ulong HealingDone;
        public ulong HealingTaken;
        
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
        
        public void AddDamageDealt(ulong damage)
        {
            DamageDealt += damage;
        }
        
        public void AddDamageTaken(ulong damage)
        {
            DamageTaken += damage;
        }
        
        public void AddHealingDone(ulong healing)
        {
            HealingDone += healing;
        }
        
        public void AddHealingTaken(ulong healing)
        {
            HealingTaken += healing;
        }
    }
    
    public struct EnemyStats
    {
        public string EnemyType;
        public int Population;
        public ulong DeathCount;
        
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