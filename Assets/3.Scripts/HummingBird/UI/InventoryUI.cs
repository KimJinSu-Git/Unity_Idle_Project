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

        private InventoryManager inventoryManager;
        private List<InventorySlot> createdSlots = new List<InventorySlot>();

        private void Awake()
        {
            inventoryManager = InventoryManager.Instance;
            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged += RefreshInventoryUI;
                inventoryManager.OnEquipmentChanged += RefreshEquippedUI;
        
                RefreshInventoryUI();
                RefreshEquippedUI();
            }
        }
        
        private void OnDestroy()
        {
            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged -= RefreshInventoryUI;
                inventoryManager.OnEquipmentChanged -= RefreshEquippedUI;
            }
        }

        private void RefreshInventoryUI()
        {
            foreach (var slot in createdSlots)
            {
                Destroy(slot.gameObject);
            }
            createdSlots.Clear();

            foreach (var item in inventoryManager.GetInventoryItems())
            {
                InventorySlot newSlot = Instantiate(slotPrefab, inventorySlotParent);
                newSlot.SetItemData(item);
                createdSlots.Add(newSlot);
            }
        }

        private void RefreshEquippedUI()
        {
            weaponSlot.SetItemData(inventoryManager.GetEquippedItem(EquipmentType.Weapon));
            armorSlot.SetItemData(inventoryManager.GetEquippedItem(EquipmentType.Armor));
        }
    }
}