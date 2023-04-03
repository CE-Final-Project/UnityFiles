using System;
using Script.CameraUtils;
using Script.Game.Action.ActionPlayers;
using Unity.Netcode;
using UnityEngine;

namespace Script.Game.GameplayObject.Character
{
    public class ClientCharacter : NetworkBehaviour
    {
        [SerializeField] private Animator clientVisualAnimator;

        public Animator OurAnimator => clientVisualAnimator;

        private ServerCharacter _serverCharacter;
        
        public bool CanPerformActions => _serverCharacter.CanPerformActions;
        
        public ServerCharacter ServerCharacter => _serverCharacter;

        private ClientActionPlayer _clientActionViz;

        private void Awake()
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
            
            _clientActionViz = new ClientActionPlayer(this);
            
            _serverCharacter = GetComponentInParent<ServerCharacter>();

            if (_serverCharacter)
            {
                clientVisualAnimator.runtimeAnimatorController = _serverCharacter.CharacterClass.AnimatorController;
            }

            gameObject.AddComponent<CameraController>();

        }
    }
}