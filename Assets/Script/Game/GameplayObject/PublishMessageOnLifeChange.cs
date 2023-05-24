using Script.Game.GameplayObject.Character;
using Script.Game.Messages;
using Script.GameState;
using Script.Infrastructure.PubSub;
using Script.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Script.Game.GameplayObject
{
    /// <summary>
    /// Server-only component which publishes a message once the LifeState changes.
    /// </summary>
    [RequireComponent(typeof(NetworkLifeState), typeof(ServerCharacter))]
    public class PublishMessageOnLifeChange : NetworkBehaviour
    {
        private NetworkLifeState _networkLifeState;
        private ServerCharacter _serverCharacter;

        [SerializeField] private string characterName;

        private NetworkNameState _nameState;

        [Inject] private IPublisher<LifeStateChangedEventMessage> _publisher;

        private void Awake()
        {
            _networkLifeState = GetComponent<NetworkLifeState>();
            _serverCharacter = GetComponent<ServerCharacter>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _nameState = GetComponent<NetworkNameState>();
                _networkLifeState.LifeState.OnValueChanged += OnLifeStateChanged;

                ServerInGameState gameState = FindObjectOfType<ServerInGameState>();
                if (gameState != null)
                {
                    gameState.Container.Inject(this);
                }
            }
        }

        private void OnLifeStateChanged(LifeState previousState, LifeState newState)
        {
            _publisher.Publish(new LifeStateChangedEventMessage()
            {
                CharacterName = _nameState != null ? _nameState.Name.Value : (FixedPlayerName)characterName,
                CharacterType = _serverCharacter.CharacterClass.CharacterType,
                NewLifeState = newState
            });
        }
    }
}
