using Unity.Netcode;
using UnityEngine;

namespace Script.Game.GameplayObject.Character
{
    public enum MovementState
    {
        Idle = 0,
        Moving = 1,
        Charging = 2,
        KnockBack = 3,
    }
    public class ServerCharacterMovement : NetworkBehaviour
    {
        [SerializeField] private Rigidbody rigidBody;
        
        private MovementState _movementState;
        
        private MovementStatus _previousState;
        
        [SerializeField] private ServerCharacter characterLogic;
        
        private float _forcedSpeed;
        private float _specialModeDurationRemaining;
        
        private Vector3 _knockBackVector;
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool TeleportModeActivated { get; set; }

        const float k_CheatSpeed = 20;

        public bool SpeedCheatActivated { get; set; }
#endif

        private void Awake()
        {
            // disable this NetworkBehavior until it is spawned
            enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                // Only enable this NetworkBehavior on the server
                enabled = true;
            }
        }
    }
}