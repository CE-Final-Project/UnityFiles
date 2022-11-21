using System;
using Survival.Game.Gameplay.GameplayObjects.Character;
using Unity.Netcode;
using UnityEngine;

namespace Survival.Game.Gameplay.GameplayObjects
{
    public class DamageReceiver : NetworkBehaviour, IDamageable
    {
        public event Action<ServerCharacter, int> DamageReceived;

        public event Action<Collision> CollisionEntered;

        [SerializeField]
        NetworkLifeState m_NetworkLifeState;

        public void ReceiveHP(ServerCharacter inflicter, int HP)
        {
            if (IsDamageable())
            {
                DamageReceived?.Invoke(inflicter, HP);
            }
        }

        public IDamageable.SpecialDamageFlags GetSpecialDamageFlags()
        {
            return IDamageable.SpecialDamageFlags.None;
        }

        public bool IsDamageable()
        {
            return m_NetworkLifeState.LifeState.Value == LifeState.Alive;
        }

        void OnCollisionEnter(Collision other)
        {
            CollisionEntered?.Invoke(other);
        }
    }
}
