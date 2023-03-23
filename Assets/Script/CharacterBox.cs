using UnityEngine;
using UnityEngine.UI;

namespace Script
{
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

        public void SetSelected(Sprite indicatorSprite)
        {
            if (indicatorSprite == null)
            {
                backgroundImage.sprite = unselectedSprite;
            }
            else
            {
                backgroundImage.sprite = indicatorSprite;
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                backgroundImage.sprite = selectedSprite;
            }
        }
    }
}