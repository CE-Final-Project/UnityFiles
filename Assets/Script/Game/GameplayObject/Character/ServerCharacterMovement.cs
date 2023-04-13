using System;
using System.Collections.Generic;
using Script.Configuration;
using Script.Game.GameplayObject.RuntimeDataContainers;
using Unity.BossRoom.Navigation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Script.Game.GameplayObject.Character
{
    public enum MovementState
    {
        Idle = 0,
        PathFollowing = 1,
        Charging = 2,
        Knockback = 3,
    }

    public class ServerCharacterMovement : NetworkBehaviour
    {
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Rigidbody2D rigidBody;
        
        private NavigationSystem _navigationSystem;
        private DynamicNavPath _navPath;
        
        private MovementState _movementState;
        
        private MovementStatus _previousState;
        
        [SerializeField] private ServerCharacter characterLogic;
        
        // when we are in charging and knockback mode, we use these additional variables
        private float _forcedSpeed;
        private float _specialModeDurationRemaining;
        private readonly NetworkVariable<bool> _isFlipped = new NetworkVariable<bool>(false);

        [SerializeField] private ContactFilter2D movementFilter;
        private readonly List<RaycastHit2D> _castCollisions = new List<RaycastHit2D>();
        private Animator _avatarAnimator;
        private Vector2 _knockBackVector; 
        // private Vector2 _movementInput = Vector2.zero;
        private static readonly int Speed = Animator.StringToHash("Speed");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool TeleportModeActivated { get; set; }

        private const float CheatSpeed = 20;

        public bool SpeedCheatActivated { get; set; }
#endif

        private void Awake()
        {
            enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                enabled = true;
                
                // On the server enable navMeshAgent and initialize
                navMeshAgent.enabled = true;
                _navigationSystem = GameObject.FindGameObjectWithTag(NavigationSystem.NavigationSystemTag).GetComponent<NavigationSystem>();
                _navPath = new DynamicNavPath(navMeshAgent, _navigationSystem);
            }
            
            _isFlipped.OnValueChanged += OnIsFlippedChanged;
            //
            // if (IsOwner)
            // {
            //     _isFlipped.Value = characterLogic.IsFlipped;
            // }
            // else
            // {
            //     Destroy(GetComponent<PlayerInput>());
            //     characterLogic.SetIsFlipped(_isFlipped.Value);
            // }
            //
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
            _movementState = MovementState.PathFollowing;
            _navPath.SetTargetPosition(position);
        }

        

        public void StartForwardCharge(float speed, float duration)
        {
            _navPath.Clear();
            _movementState = MovementState.Charging;
            _forcedSpeed = speed;
            _specialModeDurationRemaining = duration;
        }

        public void StartKnockback(Vector3 knocker, float speed, float duration)
        {
            _navPath.Clear();
            _movementState = MovementState.Knockback;
            _knockBackVector = transform.position - knocker;
            _forcedSpeed = speed;
            _specialModeDurationRemaining = duration;
        }

        /// <summary>
        /// Follow the given transform until it is reached.
        /// </summary>
        /// <param name="followTransform">The transform to follow</param>
        public void FollowTransform(Transform followTransform)
        {
            _movementState = MovementState.PathFollowing;
            _navPath.FollowTransform(followTransform);
        }

        /// <summary>
        /// Returns true if the current movement-mode is unabortable (e.g. a knockback effect)
        /// </summary>
        /// <returns></returns>
        public bool IsPerformingForcedMovement()
        {
            return _movementState is MovementState.Knockback or MovementState.Charging;
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
            _navPath?.Clear();
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
            if (!navMeshAgent.Warp(newPosition))
            {
                // warping failed! We're off the navmesh somehow. Weird... but we can still teleport
                Debug.LogWarning($"NavMeshAgent.Warp({newPosition}) failed!", gameObject);
                transform.position = newPosition;
            }

            rigidBody.position = transform.position;
            // rigidBody.rotation = transform.rotation;
        }

        // private void Update()
        // {
        //     if (IsOwner && _canMove)
        //     {
        //         _movementInput.x = Input.GetAxisRaw("Horizontal");
        //         _movementInput.y = Input.GetAxisRaw("Vertical");
        //
        //         _avatarAnimator.SetFloat(Speed, _movementInput.sqrMagnitude);
        //     }
        // }
        //
        private void FixedUpdate()
        {
            // if (_canMove)
            //     PerformMovement();
        
            PerformMovement();
            
            MovementStatus currentState = GetMovementStatus(_movementState);
            if (_previousState != currentState)
            {
                characterLogic.MovementStatus.Value = currentState;
                _previousState = currentState;
            }
        }

        public override void OnNetworkDespawn()
        {
            _navPath?.Dispose();
            
            if (IsServer)
            {
                // Disable server components when despawning
                enabled = false;
                navMeshAgent.enabled = false;
            }
        }

        // private void PerformMovement()
        // {
        //     // if (_movementState == MovementState.Idle)
        //     //     return;
        //
        //     if (!IsOwner)
        //     {
        //         return;
        //     }
        //
        //     if (_movementInput.x < 0)
        //     {
        //         _isFlipped.Value = true;
        //     }
        //
        //     if (_movementInput.x > 0)
        //     {
        //         _isFlipped.Value = false;
        //     }
        //
        //     bool isMoving = TryMove(_movementInput);
        //     
        //     _movementState = isMoving ? MovementState.Moving : MovementState.Idle;
        // }
        
        private void PerformMovement()
        {
            if (_movementState == MovementState.Idle)
                return;

            Vector3 movementVector;

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
            else if (_movementState == MovementState.Knockback)
            {
                _specialModeDurationRemaining -= Time.fixedDeltaTime;
                if (_specialModeDurationRemaining <= 0)
                {
                    _movementState = MovementState.Idle;
                    return;
                }

                float desiredMovementAmount = _forcedSpeed * Time.fixedDeltaTime;
                movementVector = _knockBackVector * desiredMovementAmount;
            }
            else
            {
                float desiredMovementAmount = GetBaseMovementSpeed() * Time.fixedDeltaTime;
                movementVector = _navPath.MoveAlongPath(desiredMovementAmount);

                // If we didn't move stop moving.
                if (movementVector == Vector3.zero)
                {
                    _movementState = MovementState.Idle;
                    return;
                }
            }

            navMeshAgent.Move(movementVector);
            
            // transform.rotation = Quaternion.LookRotation(movementVector);
            // check fliped or not
            if (movementVector.x < 0)
            {
                _isFlipped.Value = true;
            } else if (movementVector.x > 0)
            {
                _isFlipped.Value = false;
            }
            // _avatarAnimator.SetFloat(Speed, movementVector.sqrMagnitude);
            

            // After moving adjust the position of the dynamic rigidbody.
            rigidBody.position = transform.position;
        }
        
        private bool TryMove(Vector2 direction)
        {
            float speed = GetBaseMovementSpeed();
            
            int count = rigidBody.Cast(
                direction,
                movementFilter,
                _castCollisions,
                speed * Time.fixedDeltaTime + 0.01f);

            if (count == 0)
            {
                Vector2 moveVector = direction.normalized * (speed * Time.fixedDeltaTime);
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
                case MovementState.Knockback:
                    return MovementStatus.Uncontrolled;
                default:
                    return MovementStatus.Normal;
            }
        }
    }
}