using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Bird.Idle.Core;
using Bird.Idle.Data;

namespace Bird.Idle.UI
{
    /// <summary>
    /// 장비 칸 강화 및 골드 레벨업 기능을 포함하는 패널 관리
    /// </summary>
    public class EquipPanel : MonoBehaviour
    {
        [Header("Player Level Up")]
        [SerializeField] private Button levelUpButton;
        [SerializeField] private TextMeshProUGUI levelUpCostText;
        [SerializeField] private TextMeshProUGUI currentLevelText;

        // TODO: 장비 칸 강화 UI 요소 (WeaponSlot, ArmorSlot 등) 필드 추가 예정

        private CharacterManager characterManager;

        private void Awake()
        {
            characterManager = CharacterManager.Instance;
            levelUpButton.onClick.AddListener(OnLevelUpButtonClicked);
            
            if (characterManager != null)
            {
                characterManager.OnLevelUp += RefreshLevelUI;
            }
            
            RefreshLevelUI(characterManager.CharacterLevel);
        }

        private void OnLevelUpButtonClicked()
        {
            if (characterManager.TryLevelUp())
            {
                // TODO :: 성공 시 추가할 내용
            }
            
            RefreshLevelUI(characterManager.CharacterLevel); // UI 즉시 갱신
        }
        
        private void RefreshLevelUI(int currentLevel)
        {
            currentLevelText.text = $"Lv. {currentLevel:N0}";

            long goldCost = GetLevelUpCost(currentLevel); 
            
            levelUpCostText.text = $"{goldCost:N0} Gold";
            
            bool canAfford = CurrencyManager.Instance.CanAfford(CurrencyType.Gold, goldCost);
            levelUpButton.interactable = canAfford;
        }
        
        private long GetLevelUpCost(int currentLevel)
        {
            const long BASE_LEVELUP_COST = 1000;
            const long COST_MULTIPLIER = 120;
            return BASE_LEVELUP_COST + (long)currentLevel * currentLevel * COST_MULTIPLIER;
        }
    }
}