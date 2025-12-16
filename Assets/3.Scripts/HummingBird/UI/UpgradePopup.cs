using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Bird.Idle.Data;
using Bird.Idle.Gameplay;
using Bird.Idle.Core;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Bird.Idle.UI
{
    public class UpgradePopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image itemIconImage;
        [SerializeField] private Image gradeBackgroundImage; 
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI statBonusText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button equipButton;
        
        [Header("Stat Comparison Displays")]
        [SerializeField] private TextMeshProUGUI equipStatComparisonText; 
        [SerializeField] private TextMeshProUGUI collectionUpgradeStatText;

        private CollectionEntry currentEntry;
        private EquipmentData baseItemSO;
        
        private AsyncOperationHandle<Sprite> currentIconHandle;

        private void Awake()
        {
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            equipButton.onClick.AddListener(OnEquipButtonClicked);
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        private void OnDisable()
        {
            if (currentIconHandle.IsValid())
            {
                Addressables.Release(currentIconHandle);
            }
            
            baseItemSO = null;
        }
        
        public void Show(CollectionEntry entry)
        {
            currentEntry = entry;
            
            if (EquipmentCollectionManager.Instance.AllEquipmentSO.TryGetValue(entry.equipID, out EquipmentData newItemSO))
            {
                if (baseItemSO == null || baseItemSO.equipID != newItemSO.equipID)
                {
                    LoadItemIcon(newItemSO.iconAddress);
                    gradeBackgroundImage.color = GetColorByGrade(newItemSO.grade);
                }
                
                baseItemSO = newItemSO;
                
                itemNameText.text = $"{baseItemSO.equipName}";
                
                levelText.text = $"Lv. {entry.collectionLevel} -> Lv. {entry.collectionLevel + 1}";
                
                ShowEquipComparison(baseItemSO, entry.collectionLevel);
                ShowCollectionUpgradeStats(entry.collectionLevel);

                long masukCost = EquipmentCollectionManager.Instance.CalculateMasukCost(entry.collectionLevel);
                long goldCost = EquipmentCollectionManager.Instance.CalculateGoldCost(entry.collectionLevel);
                
                costText.text = $"Cost \n {masukCost:N0} Masuk \n {goldCost:N0} Gold";
                
                long currentGold = CurrencyManager.Instance.GetAmount(CurrencyType.Gold);
                long currentMasuk = CurrencyManager.Instance.GetAmount(CurrencyType.Masuk);
                
                bool canAfford = (currentMasuk >= masukCost) && (currentGold >= goldCost);
                bool isOwned = entry.count > 0;
                
                upgradeButton.interactable = canAfford && isOwned;
                
                gameObject.SetActive(true);
            }
            
            InventoryManager manager = InventoryManager.Instance;
            bool isEquipped = manager.IsItemEquipped(baseItemSO.type, baseItemSO.equipID);
            
            equipButton.GetComponentInChildren<TextMeshProUGUI>().text = isEquipped ? "UnEquiped" : "Equiped";
            
            equipButton.interactable = entry.count > 0;
        }
        
        private void LoadItemIcon(string address)
        {
            if (currentIconHandle.IsValid())
            {
                Addressables.Release(currentIconHandle);
            }

            if (itemIconImage != null)
            {
                if (string.IsNullOrEmpty(address))
                {
                    itemIconImage.sprite = null;
                    itemIconImage.enabled = false;
                    return;
                }
            }

            if (!string.IsNullOrEmpty(address))
            {
                currentIconHandle = Addressables.LoadAssetAsync<Sprite>(address);
                currentIconHandle.Completed += (handle) =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        if (itemIconImage != null)
                        {
                            itemIconImage.sprite = handle.Result;
                            itemIconImage.enabled = true;
                        }
                    }
                };
            }
        }
        
        private void ShowCollectionUpgradeStats(int currentLevel)
        {
            float upgradeAtk = baseItemSO.attackBonus * 0.05f;
            float upgradeHp = baseItemSO.healthBonus * 0.05f;
            
            collectionUpgradeStatText.text = $"+ATK: {upgradeAtk:F1}\n+HP: {upgradeHp:F1}";
            // TODO ::: 나중에, 장비 강화를 ATK, HP 말고도 강화시키고자 한다면 여기 추가.
        }
        
        private void ShowEquipComparison(EquipmentData newItem, int collectionLevel)
        {
            equipStatComparisonText.text = 
                $"ATK : {newItem.attackBonus:F1}\n" +
                $"Health : {newItem.healthBonus:F1}\n"; 
                
            // TODO ::: CritChance, ASPD 등의 비교 라인 추가
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

            bool success = EquipmentCollectionManager.Instance.TryUpgradeCollection(currentEntry.equipID);

            if (success)
            {
                Show(currentEntry);
            }
        }
        
        private Color GetColorByGrade(EquipmentGrade grade)
        {
            switch (grade)
            {
                case EquipmentGrade.Common: return Color.gray;
                case EquipmentGrade.Rare: return Color.cyan;
                case EquipmentGrade.Epic: return Color.magenta;
                case EquipmentGrade.Legendary: return Color.yellow;
                default: return Color.white;
            }
        }
    }
}