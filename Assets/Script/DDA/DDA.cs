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
        [SerializeField] GameObject enemyPrefab;
  

        float spawnDelay = 30.0f;
        int spawnCount = 6;
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

                yield return new WaitForSeconds(10); // 5 minutes

                spawnCount = 0;
                spawnDelay = 0;

                foreach (var player in PlayersStats.Instance.GetPlayerStatsMap())
                {
                    //spawnCount += player.Value.DamageDealt / 100;
                    //spawnCount += (int)Mathf.Round((1.0f * CalculateK_KillPerMinute(player.Value.KillCount) * CalculateK_DMDPerMinute(player.Value.DamageDealt)));
                    //spawnDelay += Mathf.Round((10.0f * CalculateK_KillPerMinute(player.Value.KillCount) * CalculateK_DMDPerMinute(player.Value.DamageDealt)));
                    spawnCount += (int)Mathf.Round((5.0f * CalculateK_KillPerMinute(player.Value.KillCount) * CalculateK_DMDPerMinute(player.Value.DamageDealt))/ EnemyServerCharacter.GetEnemyServerCharacters().Count);

                    spawnDelay += (int)Mathf.Round(30.0f / ( CalculateK_KillPerMinute(player.Value.KillCount) * CalculateK_DMDPerMinute(player.Value.DamageDealt )));

                    Debug.Log("Count " + spawnCount + " Delay : " + spawnDelay);
                    Debug.Log(" KPM : " + CalculateK_KillPerMinute(player.Value.KillCount) + " DMD : " + CalculateK_DMDPerMinute(player.Value.DamageDealt));


                }

                spawnDelay /= PlayersStats.Instance.GetPlayerStatsMap().Count;

                Debug.Log(PlayersStats.Instance.GetPlayerStatsMap().Count);


            }
        }

        private IEnumerator SpawnEnemyUpdate()
        {
            while (true)
            {

                _enemySpawner.SpawnEnemy(enemyPrefab, spawnDelay, spawnCount, spawnPoints, 100);
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
            float KPM = ((float)((float)KillCount / (PlayersStats.Instance.GetPlayTime()/60.0f)));

            float K_KPM = KPM / 8.7f;
            return K_KPM;
        }

        private float CalculateK_DeathPerMinute(float Death)
        {
            float K_D = Death + 0.1f / 0.67f;
            return K_D;
        }

        private float CalculateK_DMDPerMinute(float DMD)
        {
            float DMDPM = ((float)((float)DMD / (PlayersStats.Instance.GetPlayTime() / 60.0f)));

            float K_DMD = DMDPM / 957.4f; 
            return K_DMD;
        }

        private float CalculateK_DTKPerMinute()
        {
            return 1.0f;
        }




    }
}
