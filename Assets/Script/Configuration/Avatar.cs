using System;
using Script.Utils;
using UnityEngine;

namespace Script.Configuration
{
    [CreateAssetMenu]
    [Serializable]
    public class Avatar : GuidScriptableObject
    {
        public CharacterClass CharacterClass;
        public Sprite Portrait;
    }
}