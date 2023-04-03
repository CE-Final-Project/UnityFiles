using System;
using UnityEngine;

namespace Script.Utils
{
    [Serializable]
    public abstract class GuidScriptableObject : ScriptableObject
    {
        [HideInInspector]
        [SerializeField]
        private byte[] guid;

        public Guid Guid => new Guid(guid);

        private void OnValidate()
        {
            if (guid.Length == 0)
            {
                guid = Guid.NewGuid().ToByteArray();
            }
        }
    }
}