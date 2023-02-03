using Unity.Netcode;
using UnityEngine;

namespace Script.Networks
{


    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject slimePrefab;

        [SerializeField] private GameObject spawnPoints;

        private void Start()
        {
            NetworkManager.Singleton.OnServerStarted += SpawnEnemy;
        }

        private void OnDestroy()
        {
            NetworkManager.Singleton.OnServerStarted -= SpawnEnemy;
        }

        private void SpawnEnemy()
        {
            foreach (var spawnPoint in spawnPoints.GetComponentsInChildren<GameObject>())
            {
                var enemy = Instantiate(slimePrefab, spawnPoint.transform.position, Quaternion.identity);
                enemy.GetComponent<NetworkObject>().Spawn();
            }
        }

    }
}