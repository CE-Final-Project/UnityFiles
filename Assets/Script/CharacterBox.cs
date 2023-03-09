using UnityEngine;
using UnityEngine.UI;

public class CharacterBox : MonoBehaviour
{
    public Image backgroundImage;
    public string characterName;
    public Sprite unselectedSprite;
    public Sprite selectedSprite;
    public CharacterSelector characterSelector;

    private bool isSelected = false;

    private void OnMouseDown()
    {
        Debug.Log("Mouse down on " + this.name);
        characterSelector.SelectCharacter(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (isSelected)
        {
            backgroundImage.sprite = selectedSprite;
        }
        else
        {
            backgroundImage.sprite = unselectedSprite;
        }
    }
}
