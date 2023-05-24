using System.Collections.Generic;
using System.Linq;
using Script.Game.GameplayObject.RuntimeDataContainers;
using Unity.Netcode;
using UnityEngine;

namespace Script.Game.GameplayObject.Character
{
    [RequireComponent(typeof(ServerCharacter))]
    public class EnemyServerCharacter : NetworkBehaviour
    {
        [SerializeField] private ServerCharacter cachedServerCharacter;
        
        public static readonly Dictionary<string, List<ServerCharacter>> ActiveEnemies = new Dictionary<string, List<ServerCharacter>>();

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                if (!ActiveEnemies.ContainsKey(cachedServerCharacter.CharacterType.ToString()))
                {
                    ActiveEnemies.Add(cachedServerCharacter.CharacterType.ToString(), new List<ServerCharacter>{ cachedServerCharacter });
                } 
                else 
                {
                    ActiveEnemies[cachedServerCharacter.CharacterType.ToString()].Add(cachedServerCharacter);
                }
                
                GameStats.Instance.EnemiesStats.AddEnemy(cachedServerCharacter);
            }
            else
            {
                enabled = false;
            }
        }

        private void OnDisable()
        {
            if (IsServer)
            {
                GameStats.Instance.EnemiesStats.RemoveEnemy(cachedServerCharacter);
                    
                // remove enemy from list
                ActiveEnemies[cachedServerCharacter.CharacterType.ToString()].Remove(cachedServerCharacter);
            }
            
        }
        
        public static List<ServerCharacter> GetAllEnemiesServerCharacters()
        {
            return ActiveEnemies.SelectMany(x => x.Value).ToList();
        }
        
        public static List<ServerCharacter> GetEnemiesServerCharacters(string enemyType)
        {
            if (ActiveEnemies.TryGetValue(enemyType, out var enemies))
            {
                return enemies;
            }

            return new List<ServerCharacter>();
        }
    }
}