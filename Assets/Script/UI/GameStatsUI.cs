using Script.Game.GameplayObject.RuntimeDataContainers;
using Script.GameState;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Script.UI
{
    public class GameStatsUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI playerStatsText;
        [SerializeField] private TextMeshProUGUI enemyStatsText;
        
        private void Awake()
        {

            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
            }
            
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (canvasGroup.alpha == 0f)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
            
            UpdatePlayerStats();
        }

        private void UpdatePlayerStats()
        {
            if (GameStats.Instance.PlayersStats == null || canvasGroup.alpha == 0f)
            {
                return;
            }

            playerStatsText.text = GameStats.Instance.PlayersStats.GetStringPlayersStats();
        }

        public void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }
}