using Script.Game.GameplayObject;
using Script.Game.GameplayObject.Character;
using Script.Utils;
using Unity.Netcode;

namespace Script.Game.Messages
{
    public struct LifeStateChangedEventMessage : INetworkSerializeByMemcpy
    {
        public LifeState NewLifeState;
        public CharacterTypeEnum CharacterType;
        public FixedPlayerName CharacterName;
    }
}