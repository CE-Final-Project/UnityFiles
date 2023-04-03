using System;
using Script.Configuration;
using Script.Game.GameplayObject.Character;
using Script.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Avatar = Script.Configuration.Avatar;

namespace Script.Game
{
    public class NetworkAvatarGuidState : NetworkBehaviour
    {
        [HideInInspector]
        [SerializeField] public NetworkVariable<NetworkGuid> avatarNetworkGuid = new NetworkVariable<NetworkGuid>();

        [SerializeField] private AvatarRegistry avatarRegistry;
        
        private Avatar _avatar;
        
        public Avatar RegisteredAvatar
        {
            get
            {
                if (_avatar == null)
                {
                    Debug.Log($"Registering avatar {avatarNetworkGuid.Value}");
                    RegisterAvatar(avatarNetworkGuid.Value.ToGuid());
                }

                return _avatar;
            }
        }

        public void SetRandomAvatar()
        {
            avatarNetworkGuid.Value = avatarRegistry.GetRandomAvatar().Guid.ToNetworkGuid();
        }
        
        private void RegisterAvatar(Guid charGuid)
        {
            if (charGuid.Equals(Guid.Empty))
            {
                Debug.LogError("Not a valid Guid");
                return;
            }
            
            
            // based on the Guid received, Avatar is fetched from AvatarRegistry
            if (!avatarRegistry.TryGetAvatar(charGuid, out Avatar avatar))
            {
                Debug.LogError("Avatar not found!");
                return;
            }

            if (_avatar != null)
            {
                // already set, this is an idempotent call, we don't want to Instantiate twice
                return;
            }

            _avatar = avatar;
            
            if (TryGetComponent(out ServerCharacter serverCharacter))
            {
                serverCharacter.CharacterClass = avatar.CharacterClass;
            }
        }

    }
}