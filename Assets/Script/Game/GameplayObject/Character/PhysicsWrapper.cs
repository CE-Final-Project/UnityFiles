using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Script.Game.GameplayObject.Character
{
    /// <summary>
    /// Wrapper class for direct references to components relevant to physics.
    /// Each instance of a PhysicsWrapper is registered to a static dictionary, indexed by the NetworkObject's ID.
    /// </summary>
    /// <remarks>
    /// The root GameObject of PCs & NPCs is not the object which will move through the world, so other classes will
    /// need a quick reference to a PC's/NPC's in-game position.
    /// </remarks>
    public class PhysicsWrapper : NetworkBehaviour
    {
        private static readonly Dictionary<ulong, PhysicsWrapper> PhysicsWrappers = new Dictionary<ulong, PhysicsWrapper>();

        [SerializeField]
        Transform m_Transform;

        public Transform Transform => m_Transform;

        [SerializeField]
        BoxCollider2D m_DamageCollider;

        public BoxCollider2D DamageCollider => m_DamageCollider;

        ulong m_NetworkObjectID;

        public override void OnNetworkSpawn()
        {
            PhysicsWrappers.Add(NetworkObjectId, this);

            m_NetworkObjectID = NetworkObjectId;
        }

        public override void OnNetworkDespawn()
        {
            RemovePhysicsWrapper();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            RemovePhysicsWrapper();
        }

        void RemovePhysicsWrapper()
        {
            PhysicsWrappers.Remove(m_NetworkObjectID);
        }

        public static bool TryGetPhysicsWrapper(ulong networkObjectID, out PhysicsWrapper physicsWrapper)
        {
            return PhysicsWrappers.TryGetValue(networkObjectID, out physicsWrapper);
        }
    }
}
