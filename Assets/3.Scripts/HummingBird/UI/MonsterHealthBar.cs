using Bird.Idle.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Bird.Idle.UI
{
    /// <summary>
    /// 개별 몬스터의 체력 바를 관리하고 MonsterController의 상태를 반영
    /// </summary>
    public class MonsterHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        
        private MonsterController controller;

        private void Start()
        {
            controller = GetComponentInParent<MonsterController>();

            if (healthSlider == null)
            {
                healthSlider = GetComponent<Slider>();
            }
            
            if (controller == null)
            {
                Debug.LogError("[MonsterHealthBar] MonsterController를 찾을 수 없습니다.");
                return;
            }

            controller.OnHealthChanged += UpdateHealthBar;

            UpdateHealthBar();
        }
        
        public void UpdateHealthBar()
        {
            if (controller == null || healthSlider == null) return;

            float current = controller.GetCurrentHealth();
            float max = controller.GetMaxHealth();

            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }
}