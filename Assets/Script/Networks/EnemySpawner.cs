using Unity.Netcode;
using UnityEngine;

namespace Script.Networks
{


    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject slimePrefab;

        [SerializeField] private GameObject spawnPoints;

        public void SpawnEnemy()
        {
            foreach (GameObject spawnPoint in spawnPoints.GetComponentsInChildren<GameObject>())
            {
                GameObject enemy = Instantiate(slimePrefab, spawnPoint.transform.position, Quaternion.identity);
                enemy.GetComponent<NetworkObject>().Spawn();
            }
        }

    }
}