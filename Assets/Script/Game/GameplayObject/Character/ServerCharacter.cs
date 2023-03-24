using Script.Configuration;
using Script.Game.Action.ActionPlayers;
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
                if (characterClass is not null) return characterClass;
                characterClass = _state.RegisteredAvatar.CharacterClass;
                return characterClass;
            }
            set => characterClass = value;
        }
        
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
        private Action.Action startingAction;
        
        [SerializeField] private DamageReceiver damageReceiver;

        private NetworkAvatarGuidState _state;

        private void Awake()
        {
            _serverActionPlayer = new ServerActionPlayer(this);
            NetHealthState = GetComponent<NetworkHealthState>();
            NetLifeState = GetComponent<NetworkLifeState>();
            _state = GetComponent<NetworkAvatarGuidState>();
        }

    }
}