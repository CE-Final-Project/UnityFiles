using Script.Networks;
using Unity.Netcode;
using UnityEngine;
using NetworkSceneManager = Script.Networks.NetworkSceneManager;

namespace Script.SceneManagers
{
    public class InGameManager : NetworkBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject spawnPoints;
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            foreach (Transform spawnPoint in spawnPoints.GetComponentsInChildren<Transform>())
            {
                GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                enemy.GetComponent<NetworkObject>().Spawn();
            }
        }

        private void Start()
        {
            SceneTransitionHandler.Instance.SetSceneState(SceneTransitionHandler.SceneStates.InGame);
        }
    }
}