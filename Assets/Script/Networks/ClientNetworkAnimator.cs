using Unity.Netcode.Components;
using UnityEngine;

namespace Script.Networks
{
    [DisallowMultipleComponent]
    public class ClientNetworkAnimator: NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}