using System;
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
        [SerializeField] private TextMeshProUGUI topAttackText;
        [SerializeField] private TextMeshProUGUI panelAttackText;
        [SerializeField] private TextMeshProUGUI healthText;
        
        [Header("Final Combat Stat Text")]
        [SerializeField] private TextMeshProUGUI critChanceText;
        [SerializeField] private TextMeshProUGUI critDamageText;
        [SerializeField] private TextMeshProUGUI attackSpeedText;
        [SerializeField] private TextMeshProUGUI defensivePowerText;
        [SerializeField] private TextMeshProUGUI magicResistanceText;
        [SerializeField] private TextMeshProUGUI healthRegenText;
        [SerializeField] private TextMeshProUGUI evasionText;
        [SerializeField] private TextMeshProUGUI accuracyText;
        [SerializeField] private TextMeshProUGUI skillDamageText;
        [SerializeField] private TextMeshProUGUI skillCoolDownText;
        [SerializeField] private TextMeshProUGUI goldDropText;
        [SerializeField] private TextMeshProUGUI itemDropText;
        [SerializeField] private TextMeshProUGUI gemDropText;
        
        [Header("Stage Progress Slider")]
        [SerializeField] private Slider stageProgressSlider;
        [SerializeField] private TextMeshProUGUI stageProgressText;
        
        [Header("Top Bars")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI healthTextInBar;
        [SerializeField] private Slider expBar;

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
                characterManager.OnHealthChanged += UpdateHealthBar;
                characterManager.OnEXPChanged += UpdateExpBarOnly;
            }
            
            if (stageManager != null)
            {
                stageManager.OnStageProgressChanged += UpdateStageProgress;
            }
        }

        private void Start()
        {
            UpdateHealthBar();
        }

        private void OnDisable()
        {
            if (characterManager != null)
            {
                characterManager.OnLevelUp -= UpdateAllStatsUI;
                characterManager.OnStatsRecalculated -= UpdateStatsTextOnly;
                characterManager.OnHealthChanged -= UpdateHealthBar;
                characterManager.OnEXPChanged -= UpdateExpBarOnly;
            }
            if (stageManager != null)
            {
                stageManager.OnStageProgressChanged -= UpdateStageProgress;
            }
        }
        
        public void UpdateHealthBar()
        {
            if (healthBar == null) return;
            
            float current = characterManager.GetCurrentHealth;
            float max = characterManager.MaxHealth;
    
            healthBar.maxValue = max;
            healthBar.value = current;
            
            healthTextInBar.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
        
        private void UpdateExpBarOnly()
        {
            UpdateExpBar(characterManager.CharacterLevel); 
        }
        
        private void UpdateExpBar(int level)
        {
            if (expBar == null) return;
            
            long requiredExp = characterManager.GetRequiredEXP(level);
            long currentExp = characterManager.CurrentEXP;
            
            if (requiredExp == -1) // 최대 레벨
            {
                expBar.maxValue = 1f;
                expBar.value = 1f;
                return;
            }
            
            expBar.maxValue = (float)requiredExp;
            expBar.value = (float)currentExp;
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
            levelText.text = $"Lv. {level:N0}";

            UpdateStatsTextOnly();
            
            UpdateExpBar(level);
        }
        
        private void UpdateStatsTextOnly()
        {
            topAttackText.text = $"Attack: {characterManager.AttackPower:F1}";
            panelAttackText.text = $"Attack: {characterManager.AttackPower:F1}";
            healthText.text = $"Health: {characterManager.MaxHealth:F1}";
            critChanceText.text = $"Crit: {(characterManager.PlayerStats.FinalCritChance * 100):F1}%";
            critDamageText.text = $"CDMG: {(characterManager.PlayerStats.FinalCritDamage * 100):F0}%";
            attackSpeedText.text = $"ASPD: {characterManager.PlayerStats.FinalAttackSpeed:F2}x";
            defensivePowerText.text = $"DEF: {characterManager.PlayerStats.FinalDefensivePower:F0}";
            evasionText.text = $"Evasion: {(characterManager.PlayerStats.FinalEvasion * 100):F1}%";
            accuracyText.text = $"Accuracy: {(characterManager.PlayerStats.FinalAccuracy * 100):F1}%";
            healthRegenText.text = $"Regen: {characterManager.PlayerStats.FinalHealthRegen:F2}/s";
    
            goldDropText.text = $"Gold%: {((characterManager.PlayerStats.FinalGoldDrop - 1.0f) * 100):F1}%"; // 기본 100%를 제외한 증가분 표시
            
            Debug.Log("[StatsDisplay] 장비 변경으로 스탯 텍스트 UI 갱신 완료.");
        }
    }
}