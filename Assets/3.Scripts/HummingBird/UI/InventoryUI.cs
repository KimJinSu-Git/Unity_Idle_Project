using UnityEngine;
using Bird.Idle.Gameplay;
using Bird.Idle.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace Bird.Idle.UI
{
    /// <summary>
    /// 인벤토리 전체 패널을 관리하고 InventoryManager 이벤트를 구독
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("Tab Management")]
        [SerializeField] private Button weaponTabButton;
        [SerializeField] private Button armorTabButton;
        [SerializeField] private Button accessoryTabButton;
        [SerializeField] private GameObject weaponContent; 
        [SerializeField] private GameObject armorContent;
        [SerializeField] private GameObject accessoryContent;
        
        [Header("Inventory Settings")]
        [SerializeField] private Transform inventorySlotParent;
        [SerializeField] private InventorySlot slotPrefab;
        
        [Header("Static Collection Slots")]
        [SerializeField] private List<InventorySlot> allWeaponSlots; // 20개 연결
        [SerializeField] private List<InventorySlot> allArmorSlots;  // 20개 연결
        [SerializeField] private List<InventorySlot> allAccessorySlots; // 20개 연결

        private InventoryManager inventoryManager; // 장착 관리
        private EquipmentCollectionManager collectionManager; // 컬렉션 수량 관리

        private void Awake()
        {
            inventoryManager = InventoryManager.Instance;
            collectionManager = EquipmentCollectionManager.Instance;
            
            if (inventoryManager != null && collectionManager != null)
            {
                collectionManager.OnCollectionChanged += RefreshInventoryUI;
                inventoryManager.OnEquipmentChanged += RefreshEquippedUI;
        
                RefreshInventoryUI();
                RefreshEquippedUI();
            }
            
            weaponTabButton.onClick.AddListener(() => SetActiveTab(EquipmentType.Weapon));
            armorTabButton.onClick.AddListener(() => SetActiveTab(EquipmentType.Armor));
            accessoryTabButton.onClick.AddListener(() => SetActiveTab(EquipmentType.Accessory));
            
            SetActiveTab(EquipmentType.Weapon);
        }
        
        /// <summary>
        /// 선택된 장비 타입에 따라 해당하는 콘텐츠 영역만 활성화
        /// </summary>
        private void SetActiveTab(EquipmentType type)
        {
            weaponContent.SetActive(false);
            armorContent.SetActive(false);
            accessoryContent.SetActive(false);
    
            switch (type)
            {
                case EquipmentType.Weapon:
                    weaponContent.SetActive(true);
                    break;
                case EquipmentType.Armor:
                    armorContent.SetActive(true);
                    break;
                case EquipmentType.Accessory:
                    accessoryContent.SetActive(true);
                    break;
            }
        }
        
        private void OnDestroy()
        {
            if (inventoryManager != null)
            {
                if (inventoryManager != null)
                {
                    inventoryManager.OnEquipmentChanged -= RefreshEquippedUI;
                }
                if (collectionManager != null)
                {
                    collectionManager.OnCollectionChanged -= RefreshInventoryUI;
                }
            }
        }

        private void RefreshInventoryUI()
        {
            var allEquipmentSO = EquipmentCollectionManager.Instance.AllEquipmentSO;
            
            RefreshCollectionSlotsInternal(allWeaponSlots, allEquipmentSO, EquipmentType.Weapon);
            RefreshCollectionSlotsInternal(allArmorSlots, allEquipmentSO, EquipmentType.Armor);
            RefreshCollectionSlotsInternal(allAccessorySlots, allEquipmentSO, EquipmentType.Accessory);
        
            Debug.Log("[InventoryUI] 정적 컬렉션 슬롯 UI 갱신 완료.");
        }
        
        /// <summary>
        /// 지정된 타입의 슬롯 리스트를 컬렉션 데이터로 갱신
        /// </summary>
        private void RefreshCollectionSlotsInternal(List<InventorySlot> slots, Dictionary<int, EquipmentData> soDataMap, EquipmentType targetType)
        {
            var sortedEquipment = soDataMap.Values
                .Where(data => data.type == targetType)
                .OrderBy(data => (int)data.grade) 
                .ThenBy(data => data.equipID)     
                .ToList();

            for (int i = 0; i < slots.Count; i++)
            {
                if (i < sortedEquipment.Count)
                {
                    EquipmentData itemSO = sortedEquipment[i];
            
                    int count = collectionManager.GetItemCount(itemSO.equipID);
                    int level = collectionManager.GetCollectionLevel(itemSO.equipID);
            
                    slots[i].RefreshData(itemSO, count, level); 
                }
                else
                {
                    slots[i].SetEmpty();
                }
            }
        }

        private void RefreshEquippedUI()
        {
            RefreshInventoryUI();
            Debug.Log("[InventoryUI] 장착 상태 변경으로 인해 전체 인벤토리 UI 갱신.");
            
            // TODO :: 나중에, 필요한 슬롯(무기/장비/악세사리)만 순회하도록 변경하여, 비용을 줄일 수 있음.
        }
    }
}