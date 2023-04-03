using System;
using System.Collections.Generic;
using Script.Configuration;
using Script.Game.GameplayObject.RuntimeDataContainers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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
        
        [SerializeField] private float speed = 3f;
        private float _specialModeDurationRemaining;
        private readonly NetworkVariable<bool> _isFlipped = new NetworkVariable<bool>(false, default, NetworkVariableWritePermission.Owner);

        [SerializeField] private ContactFilter2D movementFilter;
        private readonly List<RaycastHit2D> _castCollisions = new List<RaycastHit2D>();
        private Animator _avatarAnimator;

        private Vector2 _knockBackVector; 
        private Vector2 _movementInput = Vector2.zero;
        private static readonly int Speed = Animator.StringToHash("Speed");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool TeleportModeActivated { get; set; }

        private const float CheatSpeed = 20;

        public bool SpeedCheatActivated { get; set; }
#endif

        public override void OnNetworkSpawn()
        {
            _isFlipped.OnValueChanged += OnIsFlippedChanged;

            if (IsOwner)
            {
                _isFlipped.Value = characterLogic.IsFlipped;
            }
            else
            {
                Destroy(GetComponent<PlayerInput>());
                characterLogic.SetIsFlipped(_isFlipped.Value);
            }

            _avatarAnimator = GetComponentInChildren<Animator>();
        }
        
        private void OnIsFlippedChanged(bool oldValue, bool newValue)
        {
            if (oldValue == newValue)
                return;
            
            characterLogic.SetIsFlipped(newValue);
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

        private void Update()
        {
            if (IsOwner)
            {
                _movementInput.x = Input.GetAxisRaw("Horizontal");
                _movementInput.y = Input.GetAxisRaw("Vertical");

                _avatarAnimator.SetFloat(Speed, _movementInput.sqrMagnitude);
            }
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
            // if (_movementState == MovementState.Idle)
            //     return;

            if (!IsOwner)
            {
                return;
            }

            if (_movementInput.x < 0)
            {
                _isFlipped.Value = true;
            }
        
            if (_movementInput.x > 0)
            {
                _isFlipped.Value = false;
            }

            bool isSuccess = TryMove(_movementInput);
        }
        
        private bool TryMove(Vector2 direction)
        {
            int count = rigidBody.Cast(
                direction,
                movementFilter,
                _castCollisions,
                speed * Time.fixedDeltaTime + 0.01f);

            if (count == 0)
            {
                Vector2 moveVector = direction * (speed * Time.fixedDeltaTime);
                rigidBody.MovePosition(rigidBody.position += moveVector);
                return true;
            }
            return false;
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