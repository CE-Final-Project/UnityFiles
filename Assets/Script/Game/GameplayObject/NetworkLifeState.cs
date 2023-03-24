using Unity.Netcode;
using UnityEngine;

namespace Script.Game.GameplayObject
{
    public enum LifeState
    {
        Alive,
        Dead
    }
    
    public class NetworkLifeState : NetworkBehaviour
    {
        [SerializeField] private NetworkVariable<LifeState> lifeState = new NetworkVariable<LifeState>(GameplayObject.LifeState.Alive);
        
        public NetworkVariable<LifeState> LifeState => lifeState;
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// Indicates whether this character is in "god mode" (cannot be damaged).
        /// </summary>
        public NetworkVariable<bool> IsGodMode { get; } = new NetworkVariable<bool>(false);
#endif
    }
}