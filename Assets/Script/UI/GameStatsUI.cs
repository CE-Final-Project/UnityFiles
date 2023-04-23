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
        [SerializeField] private CanvasGroup ddaCanvasGroup;
        [SerializeField] private TextMeshProUGUI playerStatsText;
        [SerializeField] private TextMeshProUGUI enemyStatsText;
        [SerializeField] private TextMeshProUGUI ddaStatsText;
        
        private void Awake()
        {

            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
            }
            
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            
            ddaCanvasGroup.alpha = 0f;
            ddaCanvasGroup.blocksRaycasts = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (canvasGroup.alpha == 0f && ddaCanvasGroup.alpha == 0f)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
            
            UpdatePlayerStats();
            UpdateEnemyStats();
            UpdateDDAStats();
        }

        private void UpdatePlayerStats()
        {
            if (GameStats.Instance.PlayersStats == null || canvasGroup.alpha == 0f)
            {
                return;
            }

            playerStatsText.text = GameStats.Instance.PlayersStats.GetStringPlayersStats();
        }
        
        private void UpdateEnemyStats()
        {
            if (GameStats.Instance.EnemiesStats == null || canvasGroup.alpha == 0f)
            {
                return;
            }

            enemyStatsText.text = GameStats.Instance.EnemiesStats.GetStringEnemiesStats();
        }
        
        private void UpdateDDAStats()
        {
            if (GameStats.Instance.DynamicDiffStat == null || ddaCanvasGroup.alpha == 0f)
            {
                return;
            }
            
            ddaStatsText.text = GameStats.Instance.DynamicDiffStat.GetDynamicDiffStat();
        }

        public void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            
            ddaCanvasGroup.alpha = 1f;
            ddaCanvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            
            ddaCanvasGroup.alpha = 0f;
            ddaCanvasGroup.blocksRaycasts = false;
        }
    }
}