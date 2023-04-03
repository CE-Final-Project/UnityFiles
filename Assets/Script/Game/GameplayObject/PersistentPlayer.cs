using Script.ConnectionManagement;
using Script.Game.GameplayObject.RuntimeDataContainers;
using Script.Utils;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Netcode;
using UnityEngine;

namespace Script.Game.GameplayObject
{
    [RequireComponent(typeof(NetworkObject))]
    public class PersistentPlayer : NetworkBehaviour
    {
        [SerializeField] private PersistentPlayerRuntimeCollection persistentPlayerRuntimeCollection;
        [SerializeField] private NetworkNameState networkNameState;
        [SerializeField] private NetworkAvatarGuidState networkAvatarGuidState;
        
        public NetworkNameState NetworkNameState => networkNameState;
        public NetworkAvatarGuidState NetworkAvatarGuidState => networkAvatarGuidState;

        public override void OnNetworkSpawn()
        {
            gameObject.name = "PersistentPlayer" + OwnerClientId;

            // Note that this is done here on OnNetworkSpawn in case this NetworkBehaviour's properties are accessed
            // when this element is added to the runtime collection. If this was done in OnEnable() there is a chance
            // that OwnerClientID could be its default value (0).
            persistentPlayerRuntimeCollection.Add(this);
            if (IsServer)
            {
                var sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
                if (sessionPlayerData.HasValue)
                {
                    SessionPlayerData playerData = sessionPlayerData.Value;
                    networkNameState.Name.Value = playerData.PlayerName;
                    if (playerData.HasCharacterSpawned)
                    {
                        networkAvatarGuidState.avatarNetworkGuid.Value = playerData.AvatarGuid.ToNetworkGuid();
                    }
                    else
                    {
                        networkAvatarGuidState.SetRandomAvatar();
                        playerData.AvatarGuid = networkAvatarGuidState.avatarNetworkGuid.Value.ToGuid();
                        SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
                    }
                }
            }
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            RemovePersistentPlayer();
        }
        
        public override void OnNetworkDespawn()
        {
            RemovePersistentPlayer();
        }

        private void RemovePersistentPlayer()
        {
            persistentPlayerRuntimeCollection.Remove(this);
            if (IsServer)
            {
                var sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
                if (sessionPlayerData.HasValue)
                {
                    SessionPlayerData playerData = sessionPlayerData.Value;
                    playerData.PlayerName = networkNameState.Name.Value;
                    playerData.AvatarGuid = networkAvatarGuidState.avatarNetworkGuid.Value.ToGuid();
                    SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
                }
            }
        }
    }
}