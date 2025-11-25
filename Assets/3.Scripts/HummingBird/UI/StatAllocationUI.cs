using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bird.Idle.Core;

namespace Bird.Idle.UI
{
    /// <summary>
    /// Player Stat Point 투자 로직 및 UI를 관리하는 패널 스크립트
    /// </summary>
    public class StatAllocationUI : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private TextMeshProUGUI availablePointsText;
        
        [Header("Stat Buttons & Labels")]
        [SerializeField] private Button strengthButton;
        [SerializeField] private TextMeshProUGUI strengthLabel;
        
        [SerializeField] private Button dexterityButton;
        [SerializeField] private TextMeshProUGUI dexterityLabel;

        [SerializeField] private Button intelligenceButton;
        [SerializeField] private TextMeshProUGUI intelligenceLabel;

        [SerializeField] private Button luckButton;
        [SerializeField] private TextMeshProUGUI luckLabel;

        private CharacterManager characterManager;
        
        private void Awake()
        {
            characterManager = CharacterManager.Instance;
            
            // 버튼 리스너 연결
            strengthButton.onClick.AddListener(() => TryAllocateStat("strength"));
            dexterityButton.onClick.AddListener(() => TryAllocateStat("dexterity"));
            intelligenceButton.onClick.AddListener(() => TryAllocateStat("intelligence"));
            luckButton.onClick.AddListener(() => TryAllocateStat("luck"));
        }

        private void OnEnable()
        {
            if (characterManager != null)
            {
                characterManager.OnStatsRecalculated += UpdateAllocationUI; 
                characterManager.OnLevelUp += (level) => UpdateAllocationUI(); // 레벨업 시 포인트 획득
                UpdateAllocationUI(); // 최초 갱신
            }
        }

        private void OnDisable()
        {
            if (characterManager != null)
            {
                characterManager.OnStatsRecalculated -= UpdateAllocationUI;
                characterManager.OnLevelUp -= (level) => UpdateAllocationUI();
            }
        }

        private void UpdateAllocationUI()
        {
            if (characterManager == null) return;
            
            int points = characterManager.AvailableStatPoints;
            
            availablePointsText.text = $"{points}";
            
            bool canAllocate = points > 0;
            
            strengthLabel.text = $"STR: {characterManager.Strength}";
            dexterityLabel.text = $"DEX: {characterManager.Dexterity}";
            intelligenceLabel.text = $"INT: {characterManager.Intelligence}";
            luckLabel.text = $"LUK: {characterManager.Luck}";
            
            strengthButton.interactable = canAllocate;
            dexterityButton.interactable = canAllocate;
            intelligenceButton.interactable = canAllocate;
            luckButton.interactable = canAllocate;
        }

        private void TryAllocateStat(string statName)
        {
            if (characterManager == null) return;

            if (characterManager.TryAllocateStatPoint(statName))
            {
                // 성공 시 CharacterManager 내부에서 RecalculateAllFinalStats 및 OnStatsRecalculated 호출됨
            }
            else
            {
                Debug.LogWarning("스탯 투자 실패: 포인트 부족 또는 잘못된 스탯 이름.");
            }
        }
    }
}