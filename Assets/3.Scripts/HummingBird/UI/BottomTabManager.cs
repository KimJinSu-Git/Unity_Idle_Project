using UnityEngine;
using UnityEngine.UI;
using Bird.Idle.Core;

namespace Bird.Idle.UI
{
    /// <summary>
    /// 하단 탭 전환과 메인 콘텐츠 패널 활성화 관리
    /// </summary>
    public class BottomTabManager : MonoBehaviour
    {
        [Header("Tab Buttons")]
        [SerializeField] private Button equipTabButton;
        [SerializeField] private Button inventoryTabButton;
        // TODO :: (추가 탭 버튼)

        [Header("Content Panels")]
        [SerializeField] private GameObject equipPanel;
        [SerializeField] private GameObject inventoryPanel;
        // TODO :: (추가 콘텐츠 패널)

        private void Awake()
        {
            equipTabButton.onClick.AddListener(() => SetActivePanel(equipPanel));
            inventoryTabButton.onClick.AddListener(() => SetActivePanel(inventoryPanel));

            SetActivePanel(equipPanel);
        }

        private void SetActivePanel(GameObject activePanel)
        {
            equipPanel.SetActive(false);
            inventoryPanel.SetActive(false);
            
            if (activePanel.TryGetComponent<InventoryUI>(out InventoryUI inventoryUI))
            {
                inventoryUI.ShowPanel(); 
            }
            else
            {
                activePanel.SetActive(true);
            }
        }
    }
}