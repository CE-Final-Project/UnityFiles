using UnityEngine;
using VContainer.Unity;

namespace Script.GameState
{
    public enum GameState
    {
        MainMenu,
        CharSelect,
        InGame,
        GameOver
    }
    
    public abstract class GameStateBehaviour : LifetimeScope
    {
        protected virtual bool Persists => false;
        
        protected abstract GameState ActiveState { get; }
        
        private static GameObject _activeStateGo;

        protected override void Awake()
        {
            base.Awake();
            if (Parent != null)
            {
                Parent.Container.Inject(this);
            }
        }

        protected virtual void Start()
        {
            if (_activeStateGo != null)
            {
                if (_activeStateGo == gameObject)
                {
                    return;
                }

                GameStateBehaviour previousState = _activeStateGo.GetComponent<GameStateBehaviour>();
                if (previousState.Persists && previousState.ActiveState == ActiveState)
                {
                    Destroy(gameObject);
                    return;
                }

                Destroy(_activeStateGo);
            }
            
            _activeStateGo = gameObject;
            if (Persists)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        
        protected override void OnDestroy()
        {
            if (!Persists)
            {
                _activeStateGo = null;
            }
        }
    }
}