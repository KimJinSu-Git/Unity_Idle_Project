using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Bird.Idle.Data;
using Bird.Idle.Gameplay;
using Bird.Idle.Core;

namespace Bird.Idle.UI
{
    public class UpgradePopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI statBonusText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button equipButton;

        private CollectionEntry currentEntry;
        private EquipmentData baseItemSO;
        
        // 업그레이드에 필요한 골드
        private const long GOLD_COST_PER_LEVEL = 5000; 

        private void Awake()
        {
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            equipButton.onClick.AddListener(OnEquipButtonClicked);
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        public void Show(CollectionEntry entry)
        {
            currentEntry = entry;
            
            if (EquipmentCollectionManager.Instance.AllEquipmentSO.TryGetValue(entry.equipID, out baseItemSO))
            {
                itemNameText.text = $"{baseItemSO.equipName}";
                
                levelText.text = $"Lv. {entry.collectionLevel} -> Lv. {entry.collectionLevel + 1}";
                
                float nextAtk = baseItemSO.attackBonus * 0.05f * (entry.collectionLevel + 1);
                float nextHp = baseItemSO.healthBonus * 0.05f * (entry.collectionLevel + 1);
                statBonusText.text = $"ATK: +{nextAtk:F1}\nHP: +{nextHp:F1}";

                long totalGoldCost = GOLD_COST_PER_LEVEL * (entry.collectionLevel + 1); // 레벨에 따라 비용 증가 가정
                costText.text = $"Cost: {EquipmentCollectionManager.Instance.UpgradeCostCount} number / {totalGoldCost:N0} Gold";
                
                long currentGold = CurrencyManager.Instance.GetAmount(CurrencyType.Gold);
                bool canAfford = (entry.count >= EquipmentCollectionManager.Instance.UpgradeCostCount) && (currentGold >= totalGoldCost);
                upgradeButton.interactable = canAfford;
                
                gameObject.SetActive(true);
            }
            
            InventoryManager manager = InventoryManager.Instance;
            bool isEquipped = manager.IsItemEquipped(baseItemSO.type, baseItemSO.equipID);
            
            equipButton.GetComponentInChildren<TextMeshProUGUI>().text = isEquipped ? "UnEquiped" : "Equiped";
            
            equipButton.interactable = entry.count > 0;
        }
        
        /// <summary>
        /// 장착/해제 버튼 클릭 시 InventoryManager 호출
        /// </summary>
        private void OnEquipButtonClicked()
        {
            InventoryManager manager = InventoryManager.Instance;
    
            if (manager.IsItemEquipped(baseItemSO.type, baseItemSO.equipID))
            {
                manager.UnequipItem(baseItemSO.type);
            }
            else
            {
                manager.EquipItem(baseItemSO);
            }

            Show(currentEntry);
            gameObject.SetActive(false);
        }

        private void OnUpgradeButtonClicked()
        {
            if (currentEntry == null || baseItemSO == null) return;
            
            long totalGoldCost = GOLD_COST_PER_LEVEL * (currentEntry.collectionLevel + 1);

            bool success = EquipmentCollectionManager.Instance.TryUpgradeCollection(
                currentEntry.equipID, 
                totalGoldCost);

            if (success)
            {
                gameObject.SetActive(false);
                // 성공 효과 및 로그
            }
        }
    }
}