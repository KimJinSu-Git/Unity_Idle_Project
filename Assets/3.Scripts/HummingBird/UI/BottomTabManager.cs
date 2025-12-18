using System;
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
        [SerializeField] private Button statTabButton;
        [SerializeField] private Button equipTabButton;
        [SerializeField] private Button inventoryTabButton;
        [SerializeField] private Button machineTabButton;
        // TODO :: (추가 탭 버튼)

        [Header("Content Panels")]
        [SerializeField] private GameObject statPanel;
        [SerializeField] private GameObject equipPanel;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject machinePanel;
        // TODO :: (추가 콘텐츠 패널)
        
        private GameObject currentActivePanel = null;
        public event Action<bool> OnPanelStateChanged;

        private void Awake()
        {
            statTabButton.onClick.AddListener(() => TogglePanel(statPanel));
            equipTabButton.onClick.AddListener(() => TogglePanel(equipPanel));
            inventoryTabButton.onClick.AddListener(() => TogglePanel(inventoryPanel));
            machineTabButton.onClick.AddListener(() => TogglePanel(machinePanel));

            TogglePanel(statPanel);
        }

        private void TogglePanel(GameObject targetPanel)
        {
            if (currentActivePanel == targetPanel)
            {
                CloseAllPanels();
                currentActivePanel = null;
                
                OnPanelStateChanged?.Invoke(false); 
            }
            else
            {
                CloseAllPanels();
                targetPanel.SetActive(true);
                currentActivePanel = targetPanel;

                OnPanelStateChanged?.Invoke(true);
            }
        }

        private void CloseAllPanels()
        {
            statPanel.SetActive(false);
            equipPanel.SetActive(false);
            inventoryPanel.SetActive(false);
            machinePanel.SetActive(false);
        }
    }
}