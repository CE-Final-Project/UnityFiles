using System;
using Unity.Netcode;
using VContainer;

namespace Survival.Game.Gameplay.GameState
{
    public class NetworkPostGame : NetworkBehaviour
    {
        public NetworkVariable<WinState> WinState = new NetworkVariable<WinState>();

        [Inject]
        public void Construct(PersistentGameState persistentGameState)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                WinState.Value = persistentGameState.WinState;
            }
        }
    }
}
