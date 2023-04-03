using System;
using System.Linq;
using Script.Game.GameplayObject.Character;
using UnityEngine;

namespace Script.Configuration
{
    [CreateAssetMenu]
    public class AvatarRegistry : ScriptableObject
    {
        [SerializeField] private Avatar[] _avatars;
        
        public bool TryGetAvatar(Guid charGuid, out Avatar avatarValue)
        {
            avatarValue = _avatars.FirstOrDefault(avatar => avatar.Guid == charGuid);
            return avatarValue != null;
        }
        
        public Avatar GetRandomAvatar()
        {
            if (_avatars == null || _avatars.Length == 0)
            {
                return null;
            }
            return _avatars[UnityEngine.Random.Range(0, _avatars.Length)];
        }
    }
}