using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Bird.Idle.Data;

namespace Bird.Idle.UI
{
    public class UI_GachaResultPopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button closeBackgroundButton;
        [SerializeField] private Transform itemContentParent;
        [SerializeField] private GameObject itemSlotPrefab;

        private void Awake()
        {
            if (closeBackgroundButton != null)
            {
                closeBackgroundButton.onClick.AddListener(ClosePopup);
            }
        }

        public void Show(List<EquipmentData> items)
        {
            foreach (Transform child in itemContentParent)
            {
                Destroy(child.gameObject);
            }

            if (items != null)
            {
                foreach (var item in items)
                {
                    GameObject slotObj = Instantiate(itemSlotPrefab, itemContentParent);
                    UI_RewardItemSlot slotScript = slotObj.GetComponent<UI_RewardItemSlot>();
                    
                    if (slotScript != null)
                    {
                        slotScript.SetData(item, 1);
                    }
                }
            }

            gameObject.SetActive(true);
        }

        private void ClosePopup()
        {
            gameObject.SetActive(false);
        }
    }
}