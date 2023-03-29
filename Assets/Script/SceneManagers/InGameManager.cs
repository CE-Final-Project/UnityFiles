using System.Linq;
using Script.Networks;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Script.SceneManagers
{
    public class InGameManager : NetworkBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject spawnPoints;

        [Inject] private EnemySpawner _enemySpawner;
        
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            
            _enemySpawner.SpawnEnemy(enemyPrefab, 5, 5, spawnPoints.GetComponentsInChildren<Transform>().ToList());
        }

        private void Start()
        {
            SceneTransitionHandler.Instance.SetSceneState(SceneTransitionHandler.SceneStates.InGame);
        }
    }
}