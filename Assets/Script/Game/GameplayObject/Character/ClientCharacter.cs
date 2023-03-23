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
    }
}