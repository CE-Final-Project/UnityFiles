using System;
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
        private const float SPAWN_DELAY = 3.0f;
        private const int SPAWN_COUNT = 10;
        private const int ENEMY_HP = 100;
        
        [SerializeField] List<Transform> spawnPoints;
        
        private const float CalculationInterval = 30; // Seconds

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
            var activePlayers = GameStats.Instance.PlayersStats.GetPlayerStatsMap();
            _enemySpawner.SpawnEnemy(enemyPrefab[1], SPAWN_DELAY, SPAWN_COUNT*activePlayers.Count, spawnPoints, 0); // HP = 0 will use default HP from enemy config
            
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
                spawnCount += (int)Mathf.Round((10.0f * CalculateK_KillPerMinute(player.Value.KillCount+1) * CalculateK_DMDPerMinute(player.Value.DamageDealt+1)) / (0.5f * (activeEnemies.Count+1)) * (0.5f * (CalculateK_DTKPerMinute(player.Value.DamageTaken + 1))));
               

                spawnDelay += (3.0f * ((0.25f * (activeEnemies.Count) + 1) * (0.25f * (CalculateK_DTKPerMinute(player.Value.DamageTaken + 1)))) / (CalculateK_KillPerMinute(player.Value.KillCount + 1) * CalculateK_DMDPerMinute(player.Value.DamageDealt + 1)));
                

                enemyHp += (int)Mathf.Round((20.0f * CalculateK_KillPerMinute(player.Value.KillCount+1) * CalculateK_DMDPerMinute(player.Value.DamageDealt+1)));

               

                Debug.Log("Count " + spawnCount + " Delay : " + spawnDelay + " HP : " + enemyHp);
                Debug.Log(" KPM : " + CalculateK_KillPerMinute(player.Value.KillCount) + " DMD : " + CalculateK_DMDPerMinute(player.Value.DamageDealt));
            }

            spawnDelay = (spawnDelay / activePlayers.Count);

            if (spawnCount < 3 * activePlayers.Count)
            {
                spawnCount = 3;
            }

            if (spawnCount > 15 * activePlayers.Count)
            {
                spawnCount = 15;
            }

            if (spawnDelay < 1)
            {
                spawnDelay = 1;
            }

            if (spawnDelay > 10)
            {
                spawnDelay = 10;
            }

            GameStats.Instance.DynamicDiffStat.SetSpawnCount(spawnCount);
            GameStats.Instance.DynamicDiffStat.SetSpawnDelay(spawnDelay);
            
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
            //return enemyPrefab[Random.Range(0, enemyPrefab.Count)];

            float K_KPM_AVG = 0;
            float K_DMD_AVG = 0;
            float K_DTK_AVG = 0;
            float K_HTK_AVG = 0;
            var activePlayers = GameStats.Instance.PlayersStats.GetPlayerStatsMap();

            foreach (var player in activePlayers)
            {
                K_KPM_AVG += CalculateK_KillPerMinute(player.Value.KillCount+1);
                K_DMD_AVG += CalculateK_DMDPerMinute(player.Value.DamageDealt+1);
                K_DTK_AVG += CalculateK_DTKPerMinute(player.Value.DamageTaken+1);
                //K_HTK_AVG +=  CalculateK_HealTaken(player.Value.HealingTaken+1);

                
            }

            K_KPM_AVG /= activePlayers.Count;
            K_DMD_AVG /= activePlayers.Count;
            K_DTK_AVG /= activePlayers.Count;
            K_HTK_AVG /= activePlayers.Count;
            
            GameStats.Instance.DynamicDiffStat.SetKkpmAvgValue(K_KPM_AVG);
            GameStats.Instance.DynamicDiffStat.SetKDmdAvgValue(K_DMD_AVG);
            GameStats.Instance.DynamicDiffStat.SetKDmtAvgValue(K_DTK_AVG);

            //Debug.Log(K_KPM_AVG);
            //Debug.Log(K_DMD_AVG);

            if (K_KPM_AVG > 1.5 && K_DMD_AVG > 1.5 & K_DTK_AVG < 0.8)
            {
                return enemyPrefab[2];
            }
            else if(K_KPM_AVG > 1.2 && K_DMD_AVG > 1.2 & K_DTK_AVG < 1.2)
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

            if (Math.Abs(KPM - GameStats.Instance.DynamicDiffStat.KillPerMin) > 0.00001f)
            {
                GameStats.Instance.DynamicDiffStat.SetKillPerMin(KPM);
            }

            float K_KPM = KPM / 11.2f;

            return K_KPM;
        }

        
        private float CalculateK_DMDPerMinute(float DMD)
        {
            float DMDPM = ((float)((float)(DMD) / (GameStats.Instance.PlayersStats.GetCurrentPlayTime()/ 60.0f)));
            
            if (Math.Abs(DMDPM - GameStats.Instance.DynamicDiffStat.DamageDonePerMin) > 0.00001f)
            {
                GameStats.Instance.DynamicDiffStat.SetDamageDonePerMin(DMDPM);
            }

            float K_DMD = DMDPM / 857.4f; 
            return K_DMD;2
        }

        private float CalculateK_DTKPerMinute(float DTK)
        {


            float DTKPM = (float)((float)(DTK) / (GameStats.Instance.PlayersStats.GetCurrentPlayTime() / 60.0f));

            if (Math.Abs(DTKPM - GameStats.Instance.DynamicDiffStat.DamageTakenPerMin) > 0.00001f)
            {
                GameStats.Instance.DynamicDiffStat.SetDamageTakenPerMin(DTKPM);
            }


            float K_DTKPM = DTKPM / 86.2f;

            return K_DTKPM;
        }

        private float CalculateK_HealTaken(float HTK)
        {
            float HTPM = (float)((float)(HTK) / (GameStats.Instance.PlayersStats.GetCurrentPlayTime() / 60.0f));

            float K_HTPM = HTPM / 2250.0f;

            return K_HTPM;
        }
    }
}
