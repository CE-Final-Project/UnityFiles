using Unity.Netcode;
using UnityEngine;

namespace Script.Networks
{


    public class EnemySpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;

        [SerializeField] private GameObject spawnPoints;

        

        public float spawnDelay = 5.0f;
        private float lastSpawnTime = 0.0f;

        private void Start()
        {
            lastSpawnTime = Time.time;
        }

        private void Update()
        {
            // Only the server should spawn enemies
            if (!IsServer) return;

            
            // Check if it's time to spawn a new enemy
            if (Time.time - lastSpawnTime > spawnDelay)
            {
                //print("==== Time Check ====");
                //print(lastSpawnTime);
                //print(Time.time);
                SpawnEnemy();
                lastSpawnTime = Time.time;
            }
        }

        public void SpawnEnemy()
        {
            foreach (Transform spawnPoint in spawnPoints.GetComponentsInChildren<Transform>())
            {

                if( spawnPoint.transform.childCount > 0 )
                {
                    //print("### Child > 0 ###");
                    continue;
                }
                GameObject enemy = Instantiate(enemyPrefab, spawnPoint.transform.position, Quaternion.identity);
                enemy.GetComponent<NetworkObject>().Spawn();
            }
        }

    }
}