using Script.Configuration;
using Script.Game.GameplayObject.RuntimeDataContainers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

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
        [SerializeField] private Rigidbody2D rigidBody;
        
        private MovementState _movementState;
        
        private MovementStatus _previousState;
        
        [SerializeField] private ServerCharacter characterLogic;
        
        private float _forcedSpeed;
        private float _specialModeDurationRemaining;
        
        private Vector2 _knockBackVector;
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool TeleportModeActivated { get; set; }

        private const float CheatSpeed = 20;

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
        
        /// <summary>
        /// Sets a movement target. We will path to this position, avoiding static obstacles.
        /// </summary>
        /// <param name="position">Position in world space to path to. </param>
        public void SetMovementTarget(Vector3 position)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (TeleportModeActivated)
            {
                Teleport(position);
                return;
            }
#endif
            // m_MovementState = MovementState.PathFollowing;
            // m_NavPath.SetTargetPosition(position);
        }

        public void StartForwardCharge(float speed, float duration)
        {
            // m_NavPath.Clear();
            // m_MovementState = MovementState.Charging;
            // m_ForcedSpeed = speed;
            // m_SpecialModeDurationRemaining = duration;
        }

        public void StartKnockback(Vector3 knocker, float speed, float duration)
        {
            // m_NavPath.Clear();
            // m_MovementState = MovementState.Knockback;
            // m_KnockbackVector = transform.position - knocker;
            // m_ForcedSpeed = speed;
            // m_SpecialModeDurationRemaining = duration;
        }

        /// <summary>
        /// Follow the given transform until it is reached.
        /// </summary>
        /// <param name="followTransform">The transform to follow</param>
        public void FollowTransform(Transform followTransform)
        {
            // m_MovementState = MovementState.PathFollowing;
            // m_NavPath.FollowTransform(followTransform);
        }

        /// <summary>
        /// Returns true if the current movement-mode is unabortable (e.g. a knockback effect)
        /// </summary>
        /// <returns></returns>
        public bool IsPerformingForcedMovement()
        {
            return _movementState is MovementState.KnockBack or MovementState.Charging;
        }

        /// <summary>
        /// Returns true if the character is actively moving, false otherwise.
        /// </summary>
        /// <returns></returns>
        public bool IsMoving()
        {
            return _movementState != MovementState.Idle;
        }

        /// <summary>
        /// Cancels any moves that are currently in progress.
        /// </summary>
        public void CancelMove()
        {
            // m_NavPath?.Clear();
            _movementState = MovementState.Idle;
        }

        /// <summary>
        /// Instantly moves the character to a new position. NOTE: this cancels any active movement operation!
        /// This does not notify the client that the movement occurred due to teleportation, so that needs to
        /// happen in some other way, such as with the custom action visualization in DashAttackActionFX. (Without
        /// this, the clients will animate the character moving to the new destination spot, rather than instantly
        /// appearing in the new spot.)
        /// </summary>
        /// <param name="newPosition">new coordinates the character should be at</param>
        public void Teleport(Vector3 newPosition)
        {
            CancelMove();
            // if (!m_NavMeshAgent.Warp(newPosition))
            // {
            //     // warping failed! We're off the navmesh somehow. Weird... but we can still teleport
            //     Debug.LogWarning($"NavMeshAgent.Warp({newPosition}) failed!", gameObject);
            //     transform.position = newPosition;
            // }

            rigidBody.position = transform.position;
            // rigidBody.rotation = transform.rotation;
        }

        private void FixedUpdate()
        {
            PerformMovement();

            var currentState = GetMovementStatus(_movementState);
            if (_previousState != currentState)
            {
                characterLogic.MovementStatus.Value = currentState;
                _previousState = currentState;
            }
        }

        public override void OnNetworkDespawn()
        {
            // if (m_NavPath != null)
            // {
            //     m_NavPath.Dispose();
            // }
            if (IsServer)
            {
                // Disable server components when despawning
                enabled = false;
                // m_NavMeshAgent.enabled = false;
            }
        }

        private void PerformMovement()
        {
            if (_movementState == MovementState.Idle)
                return;

            Vector2 movementVector;

            if (_movementState == MovementState.Charging)
            {
                // if we're done charging, stop moving
                _specialModeDurationRemaining -= Time.fixedDeltaTime;
                if (_specialModeDurationRemaining <= 0)
                {
                    _movementState = MovementState.Idle;
                    return;
                }

                var desiredMovementAmount = _forcedSpeed * Time.fixedDeltaTime;
                movementVector = transform.forward * desiredMovementAmount;
            }
            else if (_movementState == MovementState.KnockBack)
            {
                _specialModeDurationRemaining -= Time.fixedDeltaTime;
                if (_specialModeDurationRemaining <= 0)
                {
                    _movementState = MovementState.Idle;
                    return;
                }

                var desiredMovementAmount = _forcedSpeed * Time.fixedDeltaTime;
                movementVector = _knockBackVector * desiredMovementAmount;
            }
            else
            {
                var desiredMovementAmount = GetBaseMovementSpeed() * Time.fixedDeltaTime;
                // movementVector = m_NavPath.MoveAlongPath(desiredMovementAmount);

                // If we didn't move stop moving.
                // if (movementVector == Vector2.zero)
                // {
                //     _movementState = MovementState.Idle;
                //     return;
                // }
            }

            // m_NavMeshAgent.Move(movementVector);
            // transform.rotation = Quaternion.LookRotation(movementVector);

            // After moving adjust the position of the dynamic rigidbody.
            rigidBody.position = transform.position;
            // rigidBody.rotation = transform.rotation;
        }

        /// <summary>
        /// Retrieves the speed for this character's class.
        /// </summary>
        private float GetBaseMovementSpeed()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (SpeedCheatActivated)
            {
                return CheatSpeed;
            }
#endif
            CharacterClass characterClass = GameDataSource.Instance.CharacterDataByType[characterLogic.CharacterType];
            Assert.IsNotNull(characterClass, $"No CharacterClass data for character type {characterLogic.CharacterType}");
            return characterClass.Speed;
        }

        /// <summary>
        /// Determines the appropriate MovementStatus for the character. The
        /// MovementStatus is used by the client code when animating the character.
        /// </summary>
        private MovementStatus GetMovementStatus(MovementState movementState)
        {
            switch (movementState)
            {
                case MovementState.Idle:
                    return MovementStatus.Idle;
                case MovementState.KnockBack:
                    return MovementStatus.Uncontrolled;
                default:
                    return MovementStatus.Normal;
            }
        }
    }
}