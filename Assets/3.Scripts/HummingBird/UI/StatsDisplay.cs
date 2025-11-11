using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Bird.Idle.Core;
using Bird.Idle.Gameplay;

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
        
        [Header("Stage Progress Slider")]
        [SerializeField] private Slider stageProgressSlider;
        [SerializeField] private TextMeshProUGUI stageProgressText;

        private CharacterManager characterManager;
        private StageManager stageManager;

        private void Awake()
        {
            characterManager = CharacterManager.Instance;
            stageManager = StageManager.Instance;
            
            if (characterManager == null || stageManager == null)
            {
                Debug.LogError("[StatsDisplay] Manager 참조 실패.");
            }
        }

        private void OnEnable()
        {
            if (characterManager != null)
            {
                characterManager.OnLevelUp += UpdateAllStatsUI;
                characterManager.OnStatsRecalculated += UpdateStatsTextOnly;
            }
            
            if (stageManager != null)
            {
                stageManager.OnStageProgressChanged += UpdateStageProgress;
            }
            
            UpdateAllStatsUI(characterManager.CharacterLevel);
        }

        private void OnDisable()
        {
            if (characterManager != null)
            {
                characterManager.OnLevelUp -= UpdateAllStatsUI;
                characterManager.OnStatsRecalculated -= UpdateStatsTextOnly;
            }
            if (stageManager != null)
            {
                stageManager.OnStageProgressChanged -= UpdateStageProgress;
            }
        }
        
        public void UpdateStageProgress(int currentKills, int requiredKills, int stageID)
        {
            stageProgressSlider.minValue = 0f;
            stageProgressSlider.maxValue = (float)requiredKills; 
            stageProgressSlider.value = (float)currentKills;

            stageProgressText.text = $"{stageID}: {currentKills} / {requiredKills}";
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
        
        private void UpdateStatsTextOnly()
        {
            attackText.text = $"Attack: {characterManager.AttackPower.ToString("F1")}";
            healthText.text = $"Health: {characterManager.MaxHealth.ToString("F1")}";
    
            Debug.Log("[StatsDisplay] 장비 변경으로 스탯 텍스트 UI 갱신 완료.");
        }
    }
}