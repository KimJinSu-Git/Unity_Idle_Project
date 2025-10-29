using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Bird.Idle.Core;

namespace Bird.Idle.UI
{
    /// <summary>
    /// 플레이어의 레벨, 스탯, 경험치 바를 UI에 표시하고 CharacterManager의 이벤트를 구독
    /// </summary>
    public class StatsDisplay : MonoBehaviour
    {
        [Header("Stat Text")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI attackText;
        [SerializeField] private TextMeshProUGUI healthText;
        
        [Header("EXP Bar")]
        [SerializeField] private Slider expBarSlider;
        [SerializeField] private TextMeshProUGUI expText;

        private CharacterManager characterManager;

        private void Awake()
        {
            characterManager = CharacterManager.Instance;
            
            if (characterManager == null)
            {
                Debug.LogError("[StatsDisplay] CharacterManager 참조 실패.");
            }
        }

        private void OnEnable()
        {
            if (characterManager != null)
            {
                characterManager.OnLevelUp += UpdateAllStatsUI;
                
                characterManager.OnExpChanged += UpdateExpBarFromEvent;
                
                UpdateAllStatsUI(characterManager.CharacterLevel);
            }
        }

        private void OnDisable()
        {
            if (characterManager != null)
            {
                // 오브젝트 비활성화 시 구독 해제
                characterManager.OnLevelUp -= UpdateAllStatsUI;
                
                characterManager.OnExpChanged -= UpdateExpBarFromEvent;
            }
        }

        /// <summary>
        /// CharacterManager의 OnExpChanged 이벤트에 의해 호출
        /// </summary>
        private void UpdateExpBarFromEvent(long currentExp, long requiredExp)
        {
            expBarSlider.minValue = 0f;
            expBarSlider.maxValue = (float)requiredExp; 
            expBarSlider.value = (float)currentExp;

            expText.text = $"{currentExp:N0} / {requiredExp:N0} EXP";
        }
        
        /// <summary>
        /// 레벨 업 이벤트에 반응하여 모든 스탯 UI를 업데이트
        /// </summary>
        private void UpdateAllStatsUI(int level)
        {
            levelText.text = $"Lv. {level.ToString("N0")}";
            
            attackText.text = $"Attack: {characterManager.AttackPower.ToString("F1")}";
            healthText.text = $"Health: {characterManager.MaxHealth.ToString("F1")}";
            
            Debug.Log($"[StatsDisplay] UI 업데이트 완료: Lv.{level}, ATK:{characterManager.AttackPower}");
        }
    }
}