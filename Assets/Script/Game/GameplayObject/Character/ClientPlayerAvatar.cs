using System;
using Script.Game.GameplayObject.RuntimeDataContainers;
using Unity.Netcode;
using UnityEngine;

namespace Script.Game.GameplayObject.Character
{
    public class ClientPlayerAvatar : NetworkBehaviour
    {
        [SerializeField] private ClientPlayerAvatarRuntimeCollection playerAvatars;

        public static event Action<ClientPlayerAvatar> LocalClientSpawned;

        public static event System.Action LocalClientDespawned;

        public override void OnNetworkSpawn()
        {
            name = "PlayerAvatar" + OwnerClientId;

            if (IsClient && IsOwner)
            {
                LocalClientSpawned?.Invoke(this);
            }

            if (playerAvatars)
            {
                playerAvatars.Add(this);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient && IsOwner)
            {
                LocalClientDespawned?.Invoke();
            }

            RemoveNetworkCharacter();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            RemoveNetworkCharacter();
        }

        void RemoveNetworkCharacter()
        {
            if (playerAvatars)
            {
                playerAvatars.Remove(this);
            }
        }
    }
}
