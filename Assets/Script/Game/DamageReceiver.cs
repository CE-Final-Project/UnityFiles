using System;
using Script.Game.GameplayObject;
using Script.Game.GameplayObject.Character;
using Unity.Netcode;
using UnityEngine;

namespace Script.Game
{
    public class DamageReceiver : NetworkBehaviour, IDamageable
    {
        public event Action<ServerCharacter, int> DamageReceived;
        
        public event Action<Collision2D> CollisionEntered;

        [SerializeField] private NetworkLifeState networkLifeState;
        
        public void ReceiveHP(ServerCharacter inflicter, int hp)
        {
            if (IsDamageable())
            {
                DamageReceived?.Invoke(inflicter, hp);
            }
        }
        public IDamageable.SpecialDamageFlags GetSpecialDamageFlags()
        {
            return IDamageable.SpecialDamageFlags.None;
        }

        public bool IsDamageable()
        {
            return networkLifeState.LifeState.Value == LifeState.Alive;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            CollisionEntered?.Invoke(collision);
        }
    }
}