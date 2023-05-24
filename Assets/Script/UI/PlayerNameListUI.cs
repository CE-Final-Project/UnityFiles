using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Script.UI
{
    public class PlayerNameListUI : MonoBehaviour
    {
        public GameObject PlayerNamePrefab;
        public Transform PlayerNameListTransform;
        

        private void AddPlayerName(string playerName)
        {
            GameObject playerNameObject = Instantiate(PlayerNamePrefab, PlayerNameListTransform);
            playerNameObject.GetComponent<TextMeshProUGUI>().text = playerName;
        }
        
        public void ClearPlayerNameList()
        {
            foreach (Transform child in PlayerNameListTransform)
            {
                Destroy(child.gameObject);
            }
        }

        public void UpdatePlayerNameList(Dictionary<int, string> playerNames)
        {
            ClearPlayerNameList();
            foreach (var playerNameKvp in playerNames.OrderBy(kvp => kvp.Key))
            {
                AddPlayerName(playerNameKvp.Value);
            }
        }
    }
}