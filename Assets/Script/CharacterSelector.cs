using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    public CharacterBox[] characterBoxes;

    private void Start()
    {
        // Set all character boxes to unselected at start
        foreach (CharacterBox characterBox in characterBoxes)
        {
            characterBox.SetSelected(false);
        }
    }

    public void SelectCharacter(CharacterBox selectedCharacterBox)
    {
        // Set all character boxes to unselected except for the selected one
        foreach (CharacterBox characterBox in characterBoxes)
        {
            if (characterBox == selectedCharacterBox)
            {
                Debug.Log("Selected "+ characterBox.name);
                characterBox.SetSelected(true);
            }
            else
            {
                characterBox.SetSelected(false);
            }
        }
    }
}
