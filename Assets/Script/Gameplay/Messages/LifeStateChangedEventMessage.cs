using System;
using Survival.Game.Gameplay.GameplayObjects;
using Survival.Game.Gameplay.GameplayObjects.Character;
using Survival.Game.Utils;
using Unity.Netcode;

namespace Survival.Game.Gameplay.Messages
{
    public struct LifeStateChangedEventMessage : INetworkSerializeByMemcpy
    {
        public LifeState NewLifeState;
        public CharacterTypeEnum CharacterType;
        public FixedPlayerName CharacterName;
    }
}
