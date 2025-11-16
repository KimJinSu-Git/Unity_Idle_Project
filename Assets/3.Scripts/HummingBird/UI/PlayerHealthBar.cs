using UnityEngine;
using UnityEngine.UI;
using Bird.Idle.Core;

namespace Bird.Idle.UI
{
    /// <summary>
    /// 플레이어 캐릭터의 체력 바를 관리하고 CharacterManager 이벤트에 반응합니다.
    /// </summary>
    public class PlayerHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        
        private CharacterManager characterManager;

        private void Awake()
        {
            characterManager = CharacterManager.Instance;
            if (healthSlider == null)
            {
                healthSlider = GetComponent<Slider>();
            }
        }

        private void OnEnable()
        {
            if (characterManager != null)
            {
                characterManager.OnHealthChanged += UpdateHealthBar;
                characterManager.OnStatsRecalculated += UpdateHealthBar; 
            }
            UpdateHealthBar();
        }

        private void OnDisable()
        {
            if (characterManager != null)
            {
                characterManager.OnHealthChanged -= UpdateHealthBar;
                characterManager.OnStatsRecalculated -= UpdateHealthBar;
            }
        }
        
        public void UpdateHealthBar()
        {
            if (characterManager == null || healthSlider == null) return;
            
            float current = characterManager.GetCurrentHealth;
            float max = characterManager.MaxHealth;
            
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }
}