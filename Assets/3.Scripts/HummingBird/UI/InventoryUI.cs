using UnityEngine;
using Bird.Idle.Gameplay;
using Bird.Idle.Data;
using System.Collections.Generic;

namespace Bird.Idle.UI
{
    /// <summary>
    /// 인벤토리 전체 패널을 관리하고 InventoryManager 이벤트를 구독
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private Transform inventorySlotParent;
        [SerializeField] private InventorySlot slotPrefab;
        
        [Header("Equipped Slots")]
        [SerializeField] private InventorySlot weaponSlot;
        [SerializeField] private InventorySlot armorSlot;

        private InventoryManager inventoryManager; // 장착 관리
        private EquipmentCollectionManager collectionManager; // 컬렉션 수량 관리
        private List<InventorySlot> createdSlots = new List<InventorySlot>();

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
            foreach (var slot in createdSlots)
            {
                Destroy(slot.gameObject);
            }
            createdSlots.Clear();

            foreach (var entry in collectionManager.GetAllCollectionEntries().Values)
            {
                if (entry.count > 0)
                {
                    Debug.LogWarning($"[InventoryUI] ID {entry.equipID} (수량: {entry.count}) 컬렉션 슬롯을 생성해야 합니다.");
                }
            }
        }

        private void RefreshEquippedUI()
        {
            weaponSlot.SetItemData(inventoryManager.GetEquippedItem(EquipmentType.Weapon));
            armorSlot.SetItemData(inventoryManager.GetEquippedItem(EquipmentType.Armor));
        }
    }
}