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
    public class DDA : MonoBehaviour
    {
        [SerializeField] GameObject players;
        [SerializeField] List<GameObject> enemyPrefab;
  

        float spawnDelay = 15.0f;
        int spawnCount = 10;
        int enemyHP = 100;
        [SerializeField] List<Transform> spawnPoints;

       
       

        [Inject] private EnemySpawner _enemySpawner;

        private Coroutine _spawnCoroutine;
        private Coroutine _calculateCoroutine;

        



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
            _spawnCoroutine = StartCoroutine(SpawnEnemyUpdate());
            _calculateCoroutine = StartCoroutine(CalculatePerformance());
        }

        // call every 5 minutes
        private IEnumerator CalculatePerformance()
        {
            while (true)
            {

                yield return new WaitForSeconds(60); // 5 minutes

                spawnCount = 0;
                spawnDelay = 0;
                enemyHP = 0;
                
                var activePlayers = GameStats.Instance.PlayersStats.GetPlayerStatsMap();
                var activeEnemies = GameStats.Instance.EnemiesStats.GetEnemiesStatsMap();

                foreach (var player in activePlayers)
                {
                    //spawnCount += player.Value.DamageDealt / 100;
                    //spawnCount += (int)Mathf.Round((1.0f * CalculateK_KillPerMinute(player.Value.KillCount) * CalculateK_DMDPerMinute(player.Value.DamageDealt)));
                    //spawnDelay += Mathf.Round((10.0f * CalculateK_KillPerMinute(player.Value.KillCount) * CalculateK_DMDPerMinute(player.Value.DamageDealt)));
                    spawnCount += (int)Mathf.Round((20.0f * CalculateK_KillPerMinute(player.Value.KillCount) * CalculateK_DMDPerMinute(player.Value.DamageDealt)) / (0.5f * (activeEnemies.Count+1)));

                    spawnDelay += (int)Mathf.Round((15.0f * (0.25f * (activeEnemies.Count)+1)) / ( CalculateK_KillPerMinute(player.Value.KillCount) * CalculateK_DMDPerMinute(player.Value.DamageDealt )));

                    enemyHP += (int)Mathf.Round((100.0f * CalculateK_KillPerMinute(player.Value.KillCount) * CalculateK_DMDPerMinute(player.Value.DamageDealt)));

                    Debug.Log("Count " + spawnCount + " Delay : " + spawnDelay + " HP : " + enemyHP);
                    Debug.Log(" KPM : " + CalculateK_KillPerMinute(player.Value.KillCount) + " DMD : " + CalculateK_DMDPerMinute(player.Value.DamageDealt));

                    
                }

                spawnDelay /= activePlayers.Count;

            }
        }

        private IEnumerator SpawnEnemyUpdate()
        {
            while (true)
            {

                _enemySpawner.SpawnEnemy(enemyPrefab[0], spawnDelay, spawnCount, spawnPoints, enemyHP);
                yield return new WaitForSeconds(spawnDelay); // 5 minutes

            }
        }

        private void OnDestroy()
        {
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
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
