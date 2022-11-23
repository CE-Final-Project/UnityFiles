using System;
using Unity.Netcode;

namespace Survival.Game.Gameplay.Messages
{
    public struct DoorStateChangedEventMessage : INetworkSerializeByMemcpy
    {
        public bool IsDoorOpen;
    }
}
