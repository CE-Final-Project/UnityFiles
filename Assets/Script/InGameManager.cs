using System;
using Unity.Netcode;
using UnityEngine;

namespace Script
{
    public class InGameManager : MonoBehaviour
    {
        private void Start()
        {
            NetworkManager.Singleton.StartHost();
        }
    }
}