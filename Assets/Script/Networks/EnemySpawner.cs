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

        private void Update()
        {
            // Only the server should spawn enemies
            if (!IsServer) return;

            // Check if it's time to spawn a new enemy
            if (Time.time - lastSpawnTime > spawnDelay)
            {
                SpawnEnemy();
                lastSpawnTime = Time.time;
            }
        }

        public void SpawnEnemy()
        {
            foreach (GameObject spawnPoint in spawnPoints.GetComponentsInChildren<GameObject>())
            {
                GameObject enemy = Instantiate(enemyPrefab, spawnPoint.transform.position, Quaternion.identity);
                enemy.GetComponent<NetworkObject>().Spawn();
            }
        }

    }
}