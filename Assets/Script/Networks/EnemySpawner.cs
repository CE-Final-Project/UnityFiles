using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Script.Networks
{
    public class EnemySpawner : MonoBehaviour
    {
        public void SpawnEnemy(GameObject enemyPrefab, float spawnDelay, int spawnCount, List<Transform> spawnPoints)
        {
            StartCoroutine(SpawnEnemyCoroutine(enemyPrefab, spawnDelay, spawnCount, spawnPoints));
        }
        
        private IEnumerator SpawnEnemyCoroutine(GameObject enemyPrefab, float spawnDelay, int spawnCount, List<Transform> spawnPoints)
        {
            Debug.Assert(spawnCount != spawnPoints.Count, "SpawnCount and SpawnPoints not match!");
            
            
            for(int i = 0; i < spawnCount; i++)
            {
                int randomIndex = Random.Range(0, spawnPoints.Count);
                GameObject enemy = Instantiate(enemyPrefab, spawnPoints[randomIndex].transform.position, Quaternion.identity);
                enemy.GetComponent<NetworkObject>().Spawn();
                yield return new WaitForSeconds(1);
            }

        }
    }
}