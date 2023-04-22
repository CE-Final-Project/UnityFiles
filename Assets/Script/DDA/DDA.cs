using System.Collections;
using System.Collections.Generic;
using Script.Game.GameplayObject.Character;
using Script.Game.GameplayObject.RuntimeDataContainers;
using Script.Networks;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Script.DDA
{
    public class CalculationPerformanceResult
    {
        public int SpawnCount { get; set; }
        public float SpawnDelay { get; set; }
        public int EnemyHP { get; set; }
    }
    
    public class DDA : MonoBehaviour
    {
        [SerializeField] GameObject players;
        [SerializeField] List<GameObject> enemyPrefab;

        // Default spawn
        private const float SPAWN_DELAY = 15.0f;
        private const int SPAWN_COUNT = 10;
        private const int ENEMY_HP = 100;
        
        [SerializeField] List<Transform> spawnPoints;
        
        private const float CalculationInterval = 60; // Seconds

        [Inject] private EnemySpawner _enemySpawner;

        private Coroutine _dynamicSpawnCoroutine;


        private void Awake()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
            // spawn default enemy
            GameObject enemy = ChooseEnemy();
            _enemySpawner.SpawnEnemy(enemy, SPAWN_DELAY, SPAWN_COUNT, spawnPoints, 0); // HP = 0 will use default HP from enemy config
            
            // Start dynamic spawn
            _dynamicSpawnCoroutine = StartCoroutine(DynamicSpawnUpdate());
        }

        private IEnumerator DynamicSpawnUpdate()
        {
            while (true)
            {
                yield return new WaitForSeconds(CalculationInterval); // Wait for interval

                CalculationPerformanceResult calResult = CalculatePerformance(); // Calculate performance player

                GameObject enemy = ChooseEnemy();
                
                _enemySpawner.SpawnEnemy(enemy, calResult.SpawnDelay, calResult.SpawnCount, spawnPoints, calResult.EnemyHP);
            }
        }
        
        private CalculationPerformanceResult CalculatePerformance()
        {
            int spawnCount = 0;
            float spawnDelay = 0;
            int enemyHp = 0;
            
            var activePlayers = GameStats.Instance.PlayersStats.GetPlayerStatsMap();
            var activeEnemies = GameStats.Instance.EnemiesStats.GetEnemiesStatsMap();

            foreach (var player in activePlayers)
            {
                //spawnCount += player.Value.DamageDealt / 100;
                //spawnCount += (int)Mathf.Round((1.0f * CalculateK_KillPerMinute(player.Value.KillCount) * CalculateK_DMDPerMinute(player.Value.DamageDealt)));
                //spawnDelay += Mathf.Round((10.0f * CalculateK_KillPerMinute(player.Value.KillCount) * CalculateK_DMDPerMinute(player.Value.DamageDealt)));
                spawnCount += (int)Mathf.Round((20.0f * CalculateK_KillPerMinute(player.Value.KillCount) * CalculateK_DMDPerMinute(player.Value.DamageDealt)) / (0.5f * (activeEnemies.Count+1)));

                spawnDelay += (int)Mathf.Round((15.0f * (0.25f * (activeEnemies.Count)+1)) / ( CalculateK_KillPerMinute(player.Value.KillCount) * CalculateK_DMDPerMinute(player.Value.DamageDealt )));

                enemyHp += (int)Mathf.Round((100.0f * CalculateK_KillPerMinute(player.Value.KillCount) * CalculateK_DMDPerMinute(player.Value.DamageDealt)));

                Debug.Log("Count " + spawnCount + " Delay : " + spawnDelay + " HP : " + enemyHp);
                Debug.Log(" KPM : " + CalculateK_KillPerMinute(player.Value.KillCount) + " DMD : " + CalculateK_DMDPerMinute(player.Value.DamageDealt));
            }

            spawnDelay /= activePlayers.Count;
            
            return new CalculationPerformanceResult // return result
            {
                SpawnCount = spawnCount,
                SpawnDelay = spawnDelay,
                EnemyHP = enemyHp
            };
        }

        private GameObject ChooseEnemy()
        {
            // TODO: Choose enemy by performance
            return enemyPrefab[Random.Range(0, enemyPrefab.Count)];
        }

        private void OnDestroy()
        {
            if (_dynamicSpawnCoroutine != null)
            {
                StopCoroutine(_dynamicSpawnCoroutine);
            }
        }

        private float CalculateK_KillPerMinute(int KillCount)
        {
            float KPM = ((float)((float)(KillCount+1) / (GameStats.Instance.PlayersStats.GetCurrentPlayTime() / 60.0f)));

            float K_KPM = KPM / 6.7f;
            return K_KPM;
        }

        private float CalculateK_DeathPerMinute(float Death)
        {
            float K_D = Death + 0.1f / 0.67f;
            return K_D;
        }

        private float CalculateK_DMDPerMinute(float DMD)
        {
            float DMDPM = ((float)((float)(DMD + 1.0f) / (GameStats.Instance.PlayersStats.GetCurrentPlayTime()/ 60.0f)));

            float K_DMD = DMDPM / 957.4f; 
            return K_DMD;
        }

        private float CalculateK_DTKPerMinute()
        {
            return 1.0f;
        }




    }
}
