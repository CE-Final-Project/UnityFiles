namespace Script.GameState
{
    public class ClientInGameState : GameStateBehaviour
    {
        protected override GameState ActiveState => GameState.InGame;
        
        protected override void Awake()
        {
            base.Awake();
            
            AudioManager.Instance.StartGameMusic();
        }
    }
}