using Script.ConnectionManagement;
using VContainer;

namespace Script.GameState
{
    public class ClientInGameState : GameStateBehaviour
    {
        protected override GameState ActiveState => GameState.InGame;
        
        [Inject] private ConnectionManager _connectionManager;

        protected override void Awake()
        {
            base.Awake();
            
            AudioManager.Instance.StartGameMusic();
        }
        
        public void OnInGameMenuButtonClicked()
        {
            _connectionManager.RequestShutdown();
        }
    }
}