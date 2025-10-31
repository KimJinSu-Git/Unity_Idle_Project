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

        private EquipmentData assignedItem;

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
            if (assignedItem != null)
            {
                InventoryManager.Instance.EquipItem(assignedItem);
            }
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