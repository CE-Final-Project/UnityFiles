using Script.Game.GameplayObject.RuntimeDataContainers;
using Script.GameState;
using TMPro;
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
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        private void FixedUpdate()
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
            if (PlayersStats.Instance == null || canvasGroup.alpha == 0f)
            {
                return;
            }

            playerStatsText.text = PlayersStats.Instance.GetPlayersStats();
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