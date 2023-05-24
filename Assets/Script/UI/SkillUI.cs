using UnityEngine;
using UnityEngine.UI;

namespace Script.UI
{
    public class SkillUI : MonoBehaviour
    {
        [SerializeField] private Image m_SkillImage;
        
        public void SetSkillImage(Sprite sprite)
        {
            m_SkillImage.sprite = sprite;
        }
    }
}