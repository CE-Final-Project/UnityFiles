using System;
using UnityEngine;

namespace Script.Configuration
{
    [CreateAssetMenu]
    public class AvatarRegistry : ScriptableObject
    {
        [SerializeField] private Avatar[] _avatars;
        
        public bool TryGetAvatar(Guid guid, out Avatar avatarValue)
        {
            avatarValue = Array.Find(_avatars, avatar => avatar.Guid == guid);
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