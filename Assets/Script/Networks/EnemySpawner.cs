using System.Collections;
using System.Collections.Generic;
using Script.Game.GameplayObject.Character;
using Unity.Netcode;
using UnityEngine;

namespace Script.Networks
{
    public class EnemySpawner : MonoBehaviour
    {
        public void SpawnEnemy(
            GameObject enemyPrefab, 
            float spawnDelay, 
            int spawnCount, 
            List<Transform> spawnPoints,
            int newEnemyBaseHP = 0
        )
        {
            StartCoroutine(SpawnEnemyCoroutine(enemyPrefab, spawnDelay, spawnCount, spawnPoints, newEnemyBaseHP));
        }

        private IEnumerator SpawnEnemyCoroutine(
            GameObject enemyPrefab,
            float spawnDelay,
            int spawnCount,
            List<Transform> spawnPoints,
            int newEnemyBaseHP = 0)
        {
            //Debug.Assert(spawnCount != spawnPoints.Count, "SpawnCount and SpawnPoints not match!");
            Debug.Log("SPAWN : " + spawnCount + ", " + spawnDelay);

            for (int i = 0; i < spawnCount; i++)
            {
                int randomIndex = Random.Range(0, spawnPoints.Count);
                GameObject enemy = Instantiate(enemyPrefab, spawnPoints[randomIndex].transform.position,
                    Quaternion.identity);
                enemy.GetComponent<NetworkObject>().Spawn(true);
                
                if (newEnemyBaseHP != 0)
                {
                    // Set new enemy base HP
                    enemy.GetComponent<ServerCharacter>().SetNewHitPoints(enemy.GetComponent<ServerCharacter>().HitPoints+newEnemyBaseHP);
                }
                
                yield return new WaitForSeconds(1);
            }

        }
    }
}