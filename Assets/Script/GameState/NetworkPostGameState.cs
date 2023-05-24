using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Script.GameState
{
    public class NetworkPostGameState : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI playTimeText;

        [ClientRpc]
        public void RpcPlayTimeUpdateClientRpc(double playTime)
        {
            // convert total seconds to minutes and seconds
            int minutes = (int) playTime / 60;
            int seconds = (int) playTime % 60;
            playTimeText.text = $"Time : {minutes:00}:{seconds:00}";
        }
    }
}