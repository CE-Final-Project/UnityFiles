using System;
using Script.CameraUtils;
using Script.Configuration;
using Script.Game.Actions;
using Script.Game.Actions.ActionPlayers;
using Script.Game.Actions.Input;
using Script.Game.GameplayObject.RuntimeDataContainers;
using Script.Game.GameplayObject.UserInput;
using Script.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Script.Game.GameplayObject.Character
{
    public class ClientCharacter : NetworkBehaviour
    {
        [SerializeField] private Animator clientVisualAnimator;

        [SerializeField] private VisualizationConfiguration visualizationConfiguration;
        
        public Animator OurAnimator => clientVisualAnimator;

        private ServerCharacter _serverCharacter;
        
        public bool CanPerformActions => _serverCharacter.CanPerformActions;
        
        public ServerCharacter ServerCharacter => _serverCharacter;

        private ClientActionPlayer _clientActionViz;
        
        private PositionLerper _positionLerper;

        private PhysicsWrapper _physicsWrapper;
        
        // this value suffices for both positional and rotational interpolations; one may have a constant value for each
        const float k_LerpTime = 0.08f;

        private Vector3 _lerpedPosition;

        private bool _isHost;

        private float _currentSpeed;

        
        // [ClientRpc]
        // public void RecvDoActionClientRPC(ActionRequestData data)
        // {
        //     ActionRequestData data1 = data;
        //     _clientActionViz.PlayAction(ref data1);
        // }
        // private void Awake()
        // {
        //     enabled = false;
        // }
        //
        // public override void OnNetworkSpawn()
        // {
        //     if (!IsClient || transform.parent == null)
        //     {
        //         return;
        //     }
        //
        //     enabled = true;
        //     
        //     _clientActionViz = new ClientActionPlayer(this);
        //     
        //     _serverCharacter = GetComponentInParent<ServerCharacter>();
        //
        //     if (_serverCharacter)
        //     {
        //         clientVisualAnimator.runtimeAnimatorController = _serverCharacter.CharacterClass.AnimatorController;
        //     }
        //
        //     if (IsOwner)
        //     {
        //         gameObject.AddComponent<CameraController>();
        //     }
        //
        // }
        
        /// <summary>
        /// /// Server to Client RPC that broadcasts this action play to all clients.
        /// </summary>
        /// <param name="data"> Data about which action to play and its associated details. </param>
        [ClientRpc]
        public void RecvDoActionClientRPC(ActionRequestData data)
        {
            ActionRequestData data1 = data;
            _clientActionViz.PlayAction(ref data1);
        }

        /// <summary>
        /// This RPC is invoked on the client when the active action FXs need to be cancelled (e.g. when the character has been stunned)
        /// </summary>
        [ClientRpc]
        public void RecvCancelAllActionsClientRpc()
        {
            _clientActionViz.CancelAllActions();
        }

        /// <summary>
        /// This RPC is invoked on the client when active action FXs of a certain type need to be cancelled (e.g. when the Stealth action ends)
        /// </summary>
        [ClientRpc]
        public void RecvCancelActionsByPrototypeIDClientRpc(ActionID actionPrototypeID)
        {
            _clientActionViz.CancelAllActionsWithSamePrototypeID(actionPrototypeID);
        }

        /// <summary>
        /// Called on all clients when this character has stopped "charging up" an attack.
        /// Provides a value between 0 and 1 inclusive which indicates how "charged up" the attack ended up being.
        /// </summary>
        [ClientRpc]
        public void RecvStopChargingUpClientRpc(float percentCharged)
        {
            _clientActionViz.OnStoppedChargingUp(percentCharged);
        }

        void Awake()
        {
            enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsClient || transform.parent == null)
            {
                return;
            }

            enabled = true;

            _isHost = IsHost;

            _clientActionViz = new ClientActionPlayer(this);

            _serverCharacter = GetComponentInParent<ServerCharacter>();

            _physicsWrapper = _serverCharacter.GetComponent<PhysicsWrapper>();

            if (_serverCharacter)
            {
                clientVisualAnimator.runtimeAnimatorController = _serverCharacter.CharacterClass.AnimatorController;
            }
            
            // _serverCharacter.IsStealthy.OnValueChanged += OnStealthyChanged;
            _serverCharacter.MovementStatus.OnValueChanged += OnMovementStatusChanged;
            OnMovementStatusChanged(MovementStatus.Normal, _serverCharacter.MovementStatus.Value);

            // sync our visualization position & rotation to the most up to date version received from server
            transform.SetPositionAndRotation(_physicsWrapper.Transform.position, _physicsWrapper.Transform.rotation);
            _lerpedPosition = transform.position;
            // m_LerpedRotation = transform.rotation;

            // similarly, initialize start position and rotation for smooth lerping purposes
            _positionLerper = new PositionLerper(_physicsWrapper.Transform.position, k_LerpTime);
            // m_RotationLerper = new RotationLerper(_physicsWrapper.Transform.rotation, k_LerpTime);

            if (!_serverCharacter.IsNpc)
            {
                name = "AvatarGraphics" + _serverCharacter.OwnerClientId;

                // if (_serverCharacter.TryGetComponent(out ClientAvatarGuidHandler clientAvatarGuidHandler))
                // {
                //     m_ClientVisualsAnimator = clientAvatarGuidHandler.graphicsAnimator;
                // }
                //
                // m_CharacterSwapper = GetComponentInChildren<CharacterSwap>();

                // ...and visualize the current char-select value that we know about
                // SetAppearanceSwap();

                if (_serverCharacter.IsOwner)
                {
                    ActionRequestData data = new ActionRequestData { ActionID = GameDataSource.Instance.GeneralTargetActionPrototype.ActionID };
                    _clientActionViz.PlayAction(ref data);
                    gameObject.AddComponent<CameraController>();

                    if (_serverCharacter.TryGetComponent(out ClientInputSender inputSender))
                    {
                        // TODO: revisit; anticipated actions would play twice on the host
                        if (!IsServer)
                        {
                            inputSender.ActionInputEvent += OnActionInput;
                        }
                        inputSender.ClientMoveEvent += OnMoveInput;
                    }
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            if (_serverCharacter)
            {
                //m_NetState.DoActionEventClient -= PerformActionFX;
                //m_NetState.CancelAllActionsEventClient -= CancelAllActionFXs;
                //m_NetState.CancelActionsByPrototypeIDEventClient -= CancelActionFXByPrototypeID;
                //m_NetState.OnStopChargingUpClient -= OnStoppedChargingUpClient;
                // _serverCharacter.IsStealthy.OnValueChanged -= OnStealthyChanged;

                if (_serverCharacter.TryGetComponent(out ClientInputSender sender))
                {
                    sender.ActionInputEvent -= OnActionInput;
                    sender.ClientMoveEvent -= OnMoveInput;
                }
            }

            enabled = false;
        }

        void OnActionInput(ActionRequestData data)
        {
            _clientActionViz.AnticipateAction(ref data);
        }

        void OnMoveInput(Vector3 position)
        {
            if (!IsAnimating())
            {
                OurAnimator.SetTrigger(visualizationConfiguration.AnticipateMoveTriggerID);
            }
        }

        // void OnStealthyChanged(bool oldValue, bool newValue)
        // {
        //     SetAppearanceSwap();
        // }
        //
        // void SetAppearanceSwap()
        // {
        //     if (m_CharacterSwapper)
        //     {
        //         var specialMaterialMode = CharacterSwap.SpecialMaterialMode.None;
        //         if (_serverCharacter.IsStealthy.Value)
        //         {
        //             if (_serverCharacter.IsOwner)
        //             {
        //                 specialMaterialMode = CharacterSwap.SpecialMaterialMode.StealthySelf;
        //             }
        //             else
        //             {
        //                 specialMaterialMode = CharacterSwap.SpecialMaterialMode.StealthyOther;
        //             }
        //         }
        //
        //         m_CharacterSwapper.SwapToModel(specialMaterialMode);
        //     }
        // }

        /// <summary>
        /// Returns the value we should set the Animator's "Speed" variable, given current gameplay conditions.
        /// </summary>
        float GetVisualMovementSpeed(MovementStatus movementStatus)
        {
            if (_serverCharacter.NetLifeState.LifeState.Value != LifeState.Alive)
            {
                return visualizationConfiguration.SpeedDead;
            }

            switch (movementStatus)
            {
                case MovementStatus.Idle:
                    return visualizationConfiguration.SpeedIdle;
                case MovementStatus.Normal:
                    return visualizationConfiguration.SpeedNormal;
                case MovementStatus.Uncontrolled:
                    return visualizationConfiguration.SpeedUncontrolled;
                case MovementStatus.Slowed:
                    return visualizationConfiguration.SpeedSlowed;
                case MovementStatus.Hasted:
                    return visualizationConfiguration.SpeedHasted;
                case MovementStatus.Walking:
                    return visualizationConfiguration.SpeedWalking;
                default:
                    throw new Exception($"Unknown MovementStatus {movementStatus}");
            }
        }

        void OnMovementStatusChanged(MovementStatus previousValue, MovementStatus newValue)
        {
            _currentSpeed = GetVisualMovementSpeed(newValue);
        }

        void Update()
        {
            // On the host, Characters are translated via ServerCharacterMovement's FixedUpdate method. To ensure that
            // the game camera tracks a GameObject moving in the Update loop and therefore eliminate any camera jitter,
            // this graphics GameObject's position is smoothed over time on the host. Clients do not need to perform any
            // positional smoothing since NetworkTransform will interpolate position updates on the root GameObject.
            if (_isHost)
            {
                // Note: a cached position (m_LerpedPosition) and rotation (m_LerpedRotation) are created and used as
                // the starting point for each interpolation since the root's position and rotation are modified in
                // FixedUpdate, thus altering this transform (being a child) in the process.
                _lerpedPosition = _positionLerper.LerpPosition(_lerpedPosition,
                    _physicsWrapper.Transform.position);
                // m_LerpedRotation = m_RotationLerper.LerpRotation(m_LerpedRotation,
                //     _physicsWrapper.Transform.rotation);
                transform.SetPositionAndRotation(_lerpedPosition, Quaternion.identity);
            }

            if (clientVisualAnimator)
            {
                // set Animator variables here
                OurAnimator.SetFloat(visualizationConfiguration.SpeedVariableID, _currentSpeed);
            }

            _clientActionViz.OnUpdate();
        }

        void OnAnimEvent(string id)
        {
            //if you are trying to figure out who calls this method, it's "magic". The Unity Animation Event system takes method names as strings,
            //and calls a method of the same name on a component on the same GameObject as the Animator. See the "attack1" Animation Clip as one
            //example of where this is configured.

            _clientActionViz.OnAnimEvent(id);
        }

        public bool IsAnimating()
        {
            if (OurAnimator.GetFloat(visualizationConfiguration.SpeedVariableID) > 0.0) { return true; }

            for (int i = 0; i < OurAnimator.layerCount; i++)
            {
                if (OurAnimator.GetCurrentAnimatorStateInfo(i).tagHash != visualizationConfiguration.BaseNodeTagID)
                {
                    //we are in an active node, not the default "nothing" node.
                    return true;
                }
            }

            return false;
        }
    }
}