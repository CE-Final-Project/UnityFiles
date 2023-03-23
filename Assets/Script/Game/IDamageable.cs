using System;
using Script.Game.GameplayObject.Character;
using UnityEngine;

namespace Script.Game
{
    public interface IDamageable
    {
        void ReceiveHP(ServerCharacter inflicter, int hp);
        
        ulong NetworkObjectId { get; }
        
        Transform transform { get; }

        [Flags]
        public enum SpecialDamageFlags
        {
            None = 0,
            UnsendFlag = 1 << 0,
            StunOnTrample = 1 << 1,
            NotDamagedByPlayers = 1 << 2,
        }
        
        SpecialDamageFlags GetSpecialDamageFlags();
        
        bool IsDamageable();
    }
}