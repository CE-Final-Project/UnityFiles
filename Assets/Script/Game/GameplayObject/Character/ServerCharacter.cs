using System.Collections;
using Script.Configuration;
using Script.ConnectionManagement;
using Script.Game.Actions.ActionPlayers;
using Script.Game.Actions.Input;
using Script.Game.GameplayObject.Character.AI;
using Script.Game.GameplayObject.RuntimeDataContainers;
using Script.GameState;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Netcode;
using UnityEngine;

namespace Script.Game.GameplayObject.Character
{
    [RequireComponent(typeof(NetworkHealthState), 
        typeof(NetworkLifeState), 
        typeof(NetworkAvatarGuidState))]
    public class ServerCharacter : NetworkBehaviour, ITargetable
    {
        
        [SerializeField] private ClientCharacter clientCharacter;
        
        public ClientCharacter ClientCharacter => clientCharacter;
        
        [SerializeField] private CharacterClass characterClass;

        public CharacterClass CharacterClass
        {
            get
            {
                if (characterClass is null)
                {
                    characterClass = _state.RegisteredAvatar.CharacterClass;
                }
                return characterClass;
            }
            set => characterClass = value;
        }
        
        public PersistantGameState PersistentState { get; set; }
        
        public NetworkVariable<MovementStatus> MovementStatus { get; } = new NetworkVariable<MovementStatus>();
        
        public NetworkVariable<ulong> HeldNetworkObject { get; } = new NetworkVariable<ulong>();
        
        public NetworkHealthState NetHealthState { get; private set; }
        
        public NetworkVariable<ulong> TargetId { get; } = new NetworkVariable<ulong>();

        public int HitPoints
        {
            get => NetHealthState.HitPoints.Value;
            private set => NetHealthState.HitPoints.Value = value;
        }
        
        public NetworkLifeState NetLifeState { get; private set; }
        
        public LifeState LifeState
        {
            get => NetLifeState.LifeState.Value;
            private set => NetLifeState.LifeState.Value = value;
        }

        public bool IsNpc => CharacterClass.IsNpc;
        
        public bool IsValidTarget => LifeState != LifeState.Dead;
        
        public bool CanPerformActions => LifeState == LifeState.Alive;
        
        public CharacterTypeEnum CharacterType => CharacterClass.CharacterType;
        
        private ServerActionPlayer _serverActionPlayer;
        
        public ServerActionPlayer ActionPlayer => _serverActionPlayer;
        
        [SerializeField]
        [Tooltip("If set to false, an NPC character will be denied its brain (won't attack or chase players)")]
        private bool brainEnabled = true;

        [SerializeField]
        [Tooltip("Setting negative value disables destroying object after it is killed.")]
        private float killedDestroyDelaySeconds = 3.0f;

        [SerializeField]
        [Tooltip("If set, the ServerCharacter will automatically play the StartingAction when it is created. ")]
        private Actions.Action startingAction;
        
        [SerializeField] private DamageReceiver damageReceiver;

        [SerializeField] private ServerCharacterMovement movement;
        
        public ServerCharacterMovement Movement => movement;

        public bool IsFlipped => _spriteRenderer && _spriteRenderer.flipX;

        [SerializeField] private PhysicsWrapper physicsWrapper;
        
        public PhysicsWrapper PhysicsWrapper => physicsWrapper;

        private AIBrain _aiBrain;
        private NetworkAvatarGuidState _state;

        [SerializeField] private ServerAnimationHandler m_ServerAnimationHandler;

        public ServerAnimationHandler serverAnimationHandler => m_ServerAnimationHandler;

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _serverActionPlayer = new ServerActionPlayer(this);
            NetHealthState = GetComponent<NetworkHealthState>();
            NetLifeState = GetComponent<NetworkLifeState>();
            _state = GetComponent<NetworkAvatarGuidState>();

            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                enabled = false;
            }
            else
            {
                NetLifeState.LifeState.OnValueChanged += OnLifeStateChanged;
                damageReceiver.DamageReceived += ReceiveHP;
                damageReceiver.CollisionEntered += CollisionEntered;

                if (IsNpc)
                {
                    _aiBrain = new AIBrain(this, _serverActionPlayer);
                }
                
                if (startingAction != null)
                {
                    ActionRequestData action = new ActionRequestData
                        { ActionID = this.startingAction.ActionID };
                    PlayAction(ref action);
                }

                InitializeHitPoints();
            }
        }
        

        public override void OnNetworkDespawn()
        {
            NetLifeState.LifeState.OnValueChanged -= OnLifeStateChanged;

            if (damageReceiver)
            {
                damageReceiver.DamageReceived -= ReceiveHP;
                damageReceiver.CollisionEntered -= CollisionEntered;
            }
        }
        
        /// <summary>
        /// RPC to send inputs for this character from a client to a server.
        /// </summary>
        /// <param name="movementTarget">The position which this character should move towards.</param>
        [ServerRpc]
        public void SendCharacterInputServerRpc(Vector3 movementTarget)
        {
            if (LifeState == LifeState.Alive && !movement.IsPerformingForcedMovement())
            {
                // if we're currently playing an interruptible action, interrupt it!
                if (_serverActionPlayer.GetActiveActionInfo(out ActionRequestData data))
                {
                    if (GameDataSource.Instance.GetActionPrototypeByID(data.ActionID).Config.ActionInterruptible)
                    {
                        _serverActionPlayer.ClearActions(false);
                    }
                }
            
                _serverActionPlayer.CancelRunningActionsByLogic(ActionLogic.Target, true); //clear target on move.
                movement.SetMovementTarget(movementTarget);
            }
        }

        // ACTION SYSTEM

        /// <summary>
        /// Client->Server RPC that sends a request to play an action.
        /// </summary>
        /// <param name="data">Data about which action to play and its associated details. </param>
        [ServerRpc]
        public void RecvDoActionServerRPC(ActionRequestData data)
        {
            ActionRequestData data1 = data;
            if (!GameDataSource.Instance.GetActionPrototypeByID(data1.ActionID).Config.IsFriendly)
            {
                // notify running actions that we're using a new attack. (e.g. so Stealth can cancel itself)
                ActionPlayer.OnGameplayActivity(Actions.Action.GameplayActivity.UsingAttackAction);
            }

            PlayAction(ref data1);
        }

        // UTILITY AND SPECIAL-PURPOSE RPCs

        /// <summary>
        /// Called on server when the character's client decides they have stopped "charging up" an attack.
        /// </summary>
        [ServerRpc]
        public void RecvStopChargingUpServerRpc()
        {
            _serverActionPlayer.OnGameplayActivity(Actions.Action.GameplayActivity.StoppedChargingUp);
        }

        void InitializeHitPoints()
        {
            HitPoints = CharacterClass.BaseHP.Value;

            if (!IsNpc)
            {
                SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
                if (sessionPlayerData is { HasCharacterSpawned: true })
                {
                    HitPoints = sessionPlayerData.Value.CurrentHitPoints;
                    if (HitPoints <= 0)
                    {
                        LifeState = LifeState.Dead;
                    }
                }
            }
        }


        /// <summary>
        /// Play a sequence of actions!
        /// </summary>
        public void PlayAction(ref ActionRequestData action)
        {
            //the character needs to be alive in order to be able to play actions
            if (LifeState == LifeState.Alive && !movement.IsPerformingForcedMovement())
            {
                if (action.CancelMovement)
                {
                    movement.CancelMove();
                }

                _serverActionPlayer.PlayAction(ref action);
            }
        }

        void OnLifeStateChanged(LifeState prevLifeState, LifeState lifeState)
        {
            if (lifeState != LifeState.Alive)
            {
                _serverActionPlayer.ClearActions(true);
                movement.CancelMove();
            }
        }

        IEnumerator KilledDestroyProcess()
        {
            yield return new WaitForSeconds(killedDestroyDelaySeconds);

            if (NetworkObject != null)
            {
                NetworkObject.Despawn(true);
            }
        }

        /// <summary>
        /// Receive an HP change from somewhere. Could be healing or damage.
        /// </summary>
        /// <param name="inflicter">Person dishing out this damage/healing. Can be null. </param>
        /// <param name="HP">The HP to receive. Positive value is healing. Negative is damage.  </param>
        private void ReceiveHP(ServerCharacter inflicter, int HP)
        {
            //to our own effects, and modify the damage or healing as appropriate. But in this game, we just take it straight.
            if (HP > 0)
            {
                _serverActionPlayer.OnGameplayActivity(Actions.Action.GameplayActivity.Healed);
                float healingMod = _serverActionPlayer.GetBuffedValue(Actions.Action.BuffableValue.PercentHealingReceived);
                HP = (int)(HP * healingMod);

                // PersistentState?.PlayerStatsList[inflicter.CharacterType.ToString()].AddHealingTaken(HP);
                // PersistentState.PlayerStatsList[CharacterType.ToString()].AddHealingDone(HP);
                
                PlayersStats.Instance.AddHealingTaken(OwnerClientId, HP);
                PlayersStats.Instance.AddHealingDone(inflicter.OwnerClientId, HP);
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                // Don't apply damage if god mode is on
                if (NetLifeState.IsGodMode.Value)
                {
                    return;
                }
#endif

                _serverActionPlayer.OnGameplayActivity(Actions.Action.GameplayActivity.AttackedByEnemy);
                float damageMod = _serverActionPlayer.GetBuffedValue(Actions.Action.BuffableValue.PercentDamageReceived);
                HP = (int)(HP * damageMod);

                // PersistentState?.PlayerStatsList[CharacterType.ToString()].AddDamageTaken(-HP);
                PlayersStats.Instance.AddDamageTaken(OwnerClientId, -HP);
                PlayersStats.Instance.AddDamageDealt(inflicter.OwnerClientId, -HP);

                // serverAnimationHandler.NetworkAnimator.SetTrigger("HitReact1");
            }

            HitPoints = Mathf.Clamp(HitPoints + HP, 0, CharacterClass.BaseHP.Value);


            //let the brain know about the modified amount of damage we received.
            _aiBrain?.ReceiveHP(inflicter, HP);

            //we can't currently heal a dead character back to Alive state.
            //that's handled by a separate function.
            if (HitPoints <= 0)
            {
                if (IsNpc)
                {
                    if (killedDestroyDelaySeconds >= 0.0f && LifeState != LifeState.Dead)
                    {
                        StartCoroutine(KilledDestroyProcess());
                    }

                    LifeState = LifeState.Dead;
                }
                else
                {
                    LifeState = LifeState.Dead;
                }

                _serverActionPlayer.ClearActions(false);
            }
        }

        /// <summary>
        /// Determines a gameplay variable for this character. The value is determined
        /// by the character's active Actions.
        /// </summary>
        /// <param name="buffType"></param>
        /// <returns></returns>
        public float GetBuffedValue(Actions.Action.BuffableValue buffType)
        {
            return _serverActionPlayer.GetBuffedValue(buffType);
        }

        /// <summary>
        /// Receive a Life State change that brings Fainted characters back to Alive state.
        /// </summary>
        /// <param name="inflicter">Person reviving the character.</param>
        /// <param name="HP">The HP to set to a newly revived character.</param>
        public void Revive(ServerCharacter inflicter, int HP)
        {
            if (LifeState == LifeState.Dead)
            {
                HitPoints = Mathf.Clamp(HP, 0, CharacterClass.BaseHP.Value);
                NetLifeState.LifeState.Value = LifeState.Alive;
                PlayersStats.Instance.AddHealingDone(inflicter.OwnerClientId, HP);
            }
        }

        void Update()
        {
            _serverActionPlayer.OnUpdate();
            if (_aiBrain != null && LifeState == LifeState.Alive && brainEnabled)
            {
                _aiBrain.Update();
            }
        }

        private void CollisionEntered(Collision collision)
        {
            _serverActionPlayer?.CollisionEntered(collision);
        }

        public void SetIsFlipped(bool isFliped)
        {
            _spriteRenderer.flipX = isFliped;
        }
        
        /// <summary>
        /// This character's AIBrain. Will be null if this is not an NPC.
        /// </summary>
        public AIBrain AIBrain => _aiBrain;
    }
}