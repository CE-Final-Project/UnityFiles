using System;
using Unity.Netcode;

namespace Script.NGO
{
    public class InGameRunner : NetworkBehaviour
    {
        public void Initialize(Action onConnectionVerified, int expectedPlayerCount, Action onGameBegin, Action onGameEnd, LocalPlayer localUser)
        {
            
        }
    }
}