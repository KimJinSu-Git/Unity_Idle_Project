using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bird.Idle.Data;
using Bird.Idle.Gameplay;

namespace Bird.Idle.UI
{
    /// <summary>
    /// 인벤토리나 장착 슬롯의 개별 칸을 관리
    /// </summary>
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI gradeText;
        [SerializeField] private Button slotButton;
        
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI equipIndicator;

        private EquipmentData assignedItem;
        
        private EquipmentData itemSO;

        /// <summary>
        /// 컬렉션 UI 갱신을 위해 SO 데이터와 현재 수량/레벨을 받아오기.
        /// </summary>
        public void SetCollectionData(EquipmentData soData, int count, int level)
        {
            itemSO = soData;
            
            // TODO: Addressables 로드 로직
            iconImage.enabled = true; 
            gradeText.text = GetGradeString(itemSO.grade);
    
            // 수량 및 레벨 표시
            countText.text = $"x{count}";
            levelText.text = $"+{level}";
            
            bool isEquipped = InventoryManager.Instance.IsItemEquipped(itemSO.type, itemSO.equipID);
            SetEquippedStatus(isEquipped);
    
            slotButton.interactable = (count >= EquipmentCollectionManager.Instance.UpgradeCostCount);
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
            slotButton.interactable = false;
        }
        
        public void SetItemData(EquipmentData item)
        {
            assignedItem = item;

            if (item != null)
            {
                // TODO: Addressables를 사용해 item.iconAddress에서 아이콘 로드 로직 추가 예정
                // 임시로 아이콘 활성화
                iconImage.enabled = true; 
                gradeText.text = GetGradeString(item.grade);
                slotButton.interactable = true;
            }
            else
            {
                // 아이템이 없으면 슬롯 비활성화/초기화
                iconImage.enabled = false;
                gradeText.text = "";
                slotButton.interactable = false;
            }
        }
        
        // 클릭 시 인벤토리 매니저에 장착/사용 요청
        public void OnSlotClicked()
        {
            if (itemSO == null) return;
            
            EquipmentCollectionManager.Instance.ShowUpgradePopup(itemSO.equipID);
        }
        
        private string GetGradeString(EquipmentGrade grade)
        {
            switch (grade)
            {
                case EquipmentGrade.Legendary: return "<color=#FFA500>L</color>";
                case EquipmentGrade.Epic: return "<color=#FF00FF>E</color>";
                default: return "";
            }
        }
    }
}