using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Bird.Idle.Core;
using Bird.Idle.Data;
using Bird.Idle.Gameplay;
using Bird.Idle.Utils;
using UnityEngine.Serialization;

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
        
        [Header("Slot Enhance Displays")]
        [SerializeField] private SlotEnhanceDisplay weaponSlotDisplay;
        [SerializeField] private SlotEnhanceDisplay armorSlotDisplay;
        [SerializeField] private SlotEnhanceDisplay accessorySlotDisplay;
        
        [Header("Equipped Item Image")]
        [SerializeField] private ImageLoader equippedWeaponImageLoader;
        [SerializeField] private ImageLoader equippedArmorImageLoader;
        [SerializeField] private ImageLoader equippedAccessoryImageLoader;

        private CharacterManager characterManager;
        private SlotManager slotManager;
        
        private void OnEnable()
        {
            RefreshEquippedItemImage();
            
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnEquipmentChanged += RefreshEquippedItemImage;
            }
        }

        private void Awake()
        {
            characterManager = CharacterManager.Instance;
            slotManager = SlotManager.Instance;
            
            if (slotManager != null)
            {
                slotManager.OnSlotEnhanceChanged += InitializeSlotDisplays;
            }
            
            levelUpButton.onClick.AddListener(OnLevelUpButtonClicked);
            
            if (characterManager != null)
            {
                characterManager.OnLevelUp += RefreshLevelUI;
            }
            
            RefreshLevelUI(characterManager.CharacterLevel);
        }
        
        // 데이터 로드 완료 후 모든 슬롯 UI를 초기화하는 메서드
        private void InitializeSlotDisplays()
        {
            if (slotManager.GetSlotEnhanceData() == null) return;

            SlotEnhanceData data = slotManager.GetSlotEnhanceData();
            weaponSlotDisplay.Initialize(data);
            armorSlotDisplay.Initialize(data);
            accessorySlotDisplay.Initialize(data);
    
            slotManager.OnSlotEnhanceChanged -= InitializeSlotDisplays; 
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

            long goldCost = characterManager.GetLevelUpCost(currentLevel);
            
            if (goldCost == -1)
            {
                levelUpCostText.text = "MAX";
                levelUpButton.interactable = false;
                return;
            }
            
            levelUpCostText.text = $"{goldCost:N0} Gold";
            
            bool canAfford = CurrencyManager.Instance.CanAfford(CurrencyType.Gold, goldCost);
            levelUpButton.interactable = canAfford;
        }
        
        private void OnDisable()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnEquipmentChanged -= RefreshEquippedItemImage;
            }
        }
        
        /// <summary>
        /// InventoryManager에서 장착된 아이템 정보를 가져와 이미지를 갱신
        /// </summary>
        private void RefreshEquippedItemImage()
        {
            RefreshSingleEquipSlot(EquipmentType.Weapon, equippedWeaponImageLoader);
    
            RefreshSingleEquipSlot(EquipmentType.Armor, equippedArmorImageLoader);
    
            RefreshSingleEquipSlot(EquipmentType.Accessory, equippedAccessoryImageLoader);
        }
        
        /// <summary>
        /// 단일 장착 슬롯 이미지를 갱신
        /// </summary>
        private void RefreshSingleEquipSlot(EquipmentType type, ImageLoader loader)
        {
            if (loader == null)
            {
                return;
            }
    
            EquipmentData equippedItem = InventoryManager.Instance.GetEquippedItem(type);
    
            if (equippedItem != null)
            {
                loader.LoadSprite(equippedItem.iconAddress);
            }
            else
            {
                loader.ClearSprite();
            }
        }
    }
}