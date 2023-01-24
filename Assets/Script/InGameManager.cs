using System;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Script
{
    public class InGameManager : NetworkBehaviour
    {
        // [SerializeField] public GameObject vitualCameraPrefab;
        // [SerializeField] public GameObject playerPrefab;
        // public override void OnNetworkSpawn()
        // {
        //     base.OnNetworkSpawn();
        //     if (IsOwner)
        //     {
        //         var player = Instantiate(playerPrefab);
        //         player.GetComponent<NetworkObject>().Spawn();
        //         var vitualCamera = Instantiate(vitualCameraPrefab);
        //         vitualCamera.GetComponent<CinemachineVirtualCamera>().Follow = player.transform;
        //     }
        // }
    }
}