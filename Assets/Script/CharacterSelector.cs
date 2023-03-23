using Script.GameState;
using UnityEngine;

namespace Script
{
    public class CharacterSelector : MonoBehaviour
    {
        public CharacterBox[] characterBoxes;

        private void Start()
        {
            // Set all character boxes to unselected at start
            foreach (CharacterBox characterBox in characterBoxes)
            {
                characterBox.SetSelected(null);
            }
        }

        public void SelectCharacter(CharacterBox selectedCharacterBox)
        {
            // Set all character boxes to unselected except for the selected one
            for (int i = 0; i < characterBoxes.Length; i++)
            {
                if (characterBoxes[i] == selectedCharacterBox)
                {
                    // Debug.Log("Selected "+ characterBoxes[i].name);
                    characterBoxes[i]
                        .SetSelected(ClientCharSelectState.Instance.identifiersForEachPlayerNumber[i].Indicator);
                    ClientCharSelectState.Instance.OnPlayerClickedSeat(i);
                }
                else
                {
                    characterBoxes[i].SetSelected(null);
                }
            }
        }
    }
}