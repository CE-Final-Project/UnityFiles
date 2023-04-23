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
        private const float SPAWN_DELAY = 30.0f;
        private const int SPAWN_COUNT = 10;
        private const int ENEMY_HP = 100;
        
        [SerializeField] List<Transform> spawnPoints;
        
        private const float CalculationInterval = 10; // Seconds

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
            //GameObject enemy = ChooseEnemy();
            _enemySpawner.SpawnEnemy(enemyPrefab[1], SPAWN_DELAY, SPAWN_COUNT, spawnPoints, 0); // HP = 0 will use default HP from enemy config
            
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

            spawnCount = SPAWN_COUNT + (int)Mathf.Round((float)GameStats.Instance.PlayersStats.GetCurrentPlayTime() / 60);
            if(spawnCount > 30)
            {
                spawnCount = 30;
            }

            spawnDelay = SPAWN_DELAY - (int)Mathf.Round(0.5f * (float)GameStats.Instance.PlayersStats.GetCurrentPlayTime() / 60);
            if(spawnDelay < 5 )
            {
                spawnDelay = 5;
            }

            enemyHp = ENEMY_HP + (5 *(int)Mathf.Round(0.5f * (float)GameStats.Instance.PlayersStats.GetCurrentPlayTime() / 60));
            if(enemyHp > 200)
            {
                enemyHp = 200;
            }

            
            return new CalculationPerformanceResult // return result
            {
                SpawnCount = spawnCount,
                SpawnDelay = spawnDelay,
                EnemyHP = enemyHp
            };
        }

        private GameObject ChooseEnemy()
        {

            if ((int)Mathf.Round(0.5f * (float)GameStats.Instance.PlayersStats.GetCurrentPlayTime() / 60) > 10)
            {
                return enemyPrefab[2];
            }
            else if((int)Mathf.Round(0.5f * (float)GameStats.Instance.PlayersStats.GetCurrentPlayTime() / 60) > 5)
            {
                return enemyPrefab[0];
            }
            else
            {
                return enemyPrefab[1];
            }

            
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
            float KPM = (float)((float)(KillCount) / (GameStats.Instance.PlayersStats.GetCurrentPlayTime() / 60.0f));

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
            float DMDPM = ((float)((float)(DMD) / (GameStats.Instance.PlayersStats.GetCurrentPlayTime()/ 60.0f)));

            float K_DMD = DMDPM / 957.4f; 
            return K_DMD;
        }

        private float CalculateK_DTKPerMinute()
        {
            return 1.0f;
        }




    }
}
