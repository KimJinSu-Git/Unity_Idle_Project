using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bird.Idle.Data;
using Bird.Idle.Gameplay;
using Bird.Idle.Utils;
using JetBrains.Annotations;

namespace Bird.Idle.UI
{
    /// <summary>
    /// 인벤토리나 장착 슬롯의 개별 칸을 관리
    /// </summary>
    public class InventorySlot : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI gradeText;
        [SerializeField] private Button slotButton;
        
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI equipIndicator;
        
        [Header("Lock UI")]
        [CanBeNull] [SerializeField] private GameObject lockIconObject;
        
        [SerializeField][CanBeNull] private ImageLoader imageLoader;
        
        private EquipmentData itemSO;
        private bool isLocked = false;
        
        /// <summary>
        /// 슬롯에 SO 데이터를 바인딩하고, 수량/레벨 등 변동 데이터를 갱신
        /// </summary>
        public void RefreshData(EquipmentData soData, int count, int level)
        {
            itemSO = soData;
            
            if (iconImage != null)
            {
                iconImage.enabled = true; 
            }
            
            gradeText.text = GetGradeString(itemSO.grade);
            
            isLocked = (count <= 0);
    
            if (isLocked)
            {
                if (lockIconObject != null) lockIconObject.SetActive(true);
                
                countText.text = "";
                levelText.text = "";
                
                SetEquippedStatus(false);
            }
            else
            {
                if (lockIconObject != null) lockIconObject.SetActive(false);
                
                countText.text = $"x{count}";
                levelText.text = $"+{level}";
                
                bool isEquipped = InventoryManager.Instance.IsItemEquipped(itemSO.type, itemSO.equipID);
                SetEquippedStatus(isEquipped);
            }

            if (slotButton.interactable == false)
            {
                slotButton.interactable = true;
            }
        }
        
        /// <summary>
        /// 장착 상태(E 표시)를 설정
        /// </summary>
        public void SetEquippedStatus(bool isEquipped)
        {
            if (equipIndicator != null)
            {
                equipIndicator.gameObject.SetActive(isEquipped);
            }
        }
        
        /// <summary>
        /// 슬롯을 빈 상태로 초기화
        /// </summary>
        public void SetEmpty()
        {
            iconImage.enabled = false;
            gradeText.text = "";
            countText.text = "";
            levelText.text = "";
            lockIconObject.SetActive(false);
            slotButton.interactable = false;
            isLocked = false;
        }
        
        public void OnSlotClicked()
        {
            if (itemSO == null) return;
            
            if (isLocked)
            {
                if (UI_ToastMessage.Instance != null)
                {
                    UI_ToastMessage.Instance.Show("Get Item Please.");
                }
                return;
            }
            
            EquipmentCollectionManager.Instance.ShowUpgradePopup(itemSO.equipID);
        }
        
        private string GetGradeString(EquipmentGrade grade)
        {
            switch (grade)
            {
                case EquipmentGrade.Common:    return "<color=#808080>C</color>";
                case EquipmentGrade.Rare:      return "<color=#00FFFF>R</color>";
                case EquipmentGrade.Epic: return "<color=#FF00FF>E</color>";
                case EquipmentGrade.Legendary: return "<color=#FFA500>L</color>";
                default: return "";
            }
        }
    }
}