using System;
using Script.Configuration;
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
        public NetworkVariable<NetworkGuid> AvatarGuid = new NetworkVariable<NetworkGuid>();
        
        [FormerlySerializedAs("_avatarRegistry")] [SerializeField] private AvatarRegistry avatarRegistry;
        
        private Avatar _avatar;
        
        public Avatar RegisteredAvatar
        {
            get
            {
                if (_avatar == null)
                {
                    RegisterAvatar(AvatarGuid.Value.ToGuid());
                }

                return _avatar;
            }
        }
        
        public void SetRandomAvatar()
        {
            AvatarGuid.Value = avatarRegistry.GetRandomAvatar().Guid.ToNetworkGuid();
        }
        
        private void RegisterAvatar(Guid guid)
        {
            if (guid.Equals(Guid.Empty))
            {
                // not a valid Guid
                return;
            }

            // based on the Guid received, Avatar is fetched from AvatarRegistry
            if (!avatarRegistry.TryGetAvatar(guid, out Avatar avatar))
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
            
            // if (TryGetComponent<ServerCharacter>(out ServerCharacter serverCharacter))
            // {
            //     serverCharacter.CharacterClass = avatar.CharacterClass;
            // }
        }

    }
}