using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Bird.Idle.Data;
using Bird.Idle.Gameplay;
using Bird.Idle.Core;

namespace Bird.Idle.UI
{
    /// <summary>
    /// 장비 칸 강화 슬롯 하나를 표시하고 강화 요청 처리
    /// </summary>
    public class SlotEnhanceDisplay : MonoBehaviour
    {
        [SerializeField] private EquipmentType enhanceType; // 이 슬롯이 담당할 장비 타입
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI statText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button enhanceButton;

        private SlotManager slotManager;
        
        private void Awake()
        {
            slotManager = SlotManager.Instance;
            enhanceButton.onClick.AddListener(OnEnhanceButtonClicked);

            // TODO: SlotEnhanceData Addressables 로드가 완료된 후 참조 방식 구현 예정
        }
        
        public void Initialize()
        {
            if (slotManager == null) slotManager = SlotManager.Instance;
            
            RefreshUI();
            slotManager.OnSlotEnhanceChanged += RefreshUI;
        }

        private void OnDestroy()
        {
            if (slotManager != null)
            {
                slotManager.OnSlotEnhanceChanged -= RefreshUI;
            }
        }

        public void RefreshUI()
        {
            SlotEnhanceData currentData = slotManager.GetSlotEnhanceData();
            if (currentData == null) 
            {
                Debug.LogWarning($"[SlotEnhanceDisplay] {enhanceType} 데이터 로드 대기 중. UI 갱신 스킵.");
                return;
            }
            
            int currentLevel = slotManager.GetSlotLevel(enhanceType);
            SlotEnhanceData.SlotEnhanceEntry nextEntry = currentData.GetEnhanceEntry(enhanceType, currentLevel);

            levelText.text = $"{enhanceType} Lv. {currentLevel}";
            
            if (nextEntry.EnhanceLevel != -1)
            {
                // 다음 스탯 및 비용 표시
                statText.text = $"+ATK: {nextEntry.AttackIncrease:F1} | +HP: {nextEntry.HealthIncrease:F1}";
                costText.text = $"{nextEntry.MasukCost:N0} Masuk ({nextEntry.SuccessRate * 100}%)";
                
                enhanceButton.interactable = true;
            }
            else
            {
                statText.text = "Max Level";
                costText.text = "MAX";
            }
        }

        private void OnEnhanceButtonClicked()
        {
            slotManager.TryEnhanceSlot(enhanceType);
            
            RefreshUI(); 
        }
    }
}