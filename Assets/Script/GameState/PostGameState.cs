using Script.ConnectionManagement;
using Script.Game.GameplayObject.RuntimeDataContainers;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Script.GameState
{
    public class PostGameState : GameStateBehaviour
    {
        [SerializeField] private NetcodeHooks netCodeHooks;
        
        [SerializeField] private NetworkPostGameState networkPostGameState;
        
        [Inject] private ConnectionManager _connectionManager;

        protected override GameState ActiveState => GameState.GameOver;

        protected override void Awake()
        {
            base.Awake();
            netCodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            
            AudioManager.Instance.StartPostGameMusic();
        }
        
        private void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }
            
            var playTime = GameStats.Instance.PlayersStats.GetCurrentPlayTime();
            networkPostGameState.RpcPlayTimeUpdateClientRpc(playTime);
        }

        public void OnMainMenuButtonClicked()
        {
            _connectionManager.RequestShutdown();
        }
    }
}