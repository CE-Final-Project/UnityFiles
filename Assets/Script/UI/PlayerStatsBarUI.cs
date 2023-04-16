using UnityEngine;
using UnityEngine.UI;

namespace Script.UI
{
    public class PlayerStatsBarUI : MonoBehaviour
    {
        [SerializeField] private Slider m_HealthSlider;
        [SerializeField] private Image m_CharacterImage;
        [SerializeField] private HorizontalLayoutGroup m_SkillLayoutGroup;
        [SerializeField] private GameObject m_SkillPrefab;
        
        public void SetHealth(int health)
        {
            m_HealthSlider.value = health;
        }

        public void SetMaxHealth(int maxHealth)
        {
            m_HealthSlider.maxValue = maxHealth;
            m_HealthSlider.value = maxHealth;
        }
        
        public void SetCharacterImage(Sprite sprite)
        {
            m_CharacterImage.sprite = sprite;
        }
        
        public void AddSkill(Sprite skillSprite)
        {
            Transform skillsLayoutGroupTransform = m_SkillLayoutGroup.transform;
            GameObject skill = Instantiate(m_SkillPrefab, skillsLayoutGroupTransform);
            skill.GetComponent<SkillUI>().SetSkillImage(skillSprite);
        }
    }
}