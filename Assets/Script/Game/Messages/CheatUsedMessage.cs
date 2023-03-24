using Script.Utils;
using Unity.Collections;
using Unity.Netcode;

namespace Script.Game.Messages
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD

    public struct CheatUsedMessage : INetworkSerializeByMemcpy
    {
        private FixedString32Bytes _cheatUsed;
        private FixedPlayerName _cheaterName;

        public string CheatUsed => _cheatUsed.ToString();
        public string CheaterName => _cheaterName.ToString();

        public CheatUsedMessage(string cheatUsed, string cheaterName)
        {
            _cheatUsed = cheatUsed;
            _cheaterName = cheaterName;
        }
    }

#endif
}
