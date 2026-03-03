using UnityEngine;
using TMPro;
using Bird.Idle.Data;
using System;
using System.Collections.Generic;
using Bird.Idle.Utils;
using UnityEngine.UI;

namespace Bird.Idle.UI
{
    /// <summary>
    /// 오프라인 보상 정보를 표시하고 보상 수령 후 게임을 재개하는 팝업 UI
    /// </summary>
    public class AFKRewardPopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI durationText;
        [SerializeField] private TextMeshProUGUI goldRewardText;
        [SerializeField] private TextMeshProUGUI masukRewardText;
        [SerializeField] private Button confirmButton;
        
        [Header("Item List Settings")]
        [SerializeField] private Transform itemContentParent;
        [SerializeField] private GameObject itemSlotPrefab;

        private TimeSpan idleDuration;
        private long rewardedGold;

        private void Awake()
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 팝업을 열고 보상 정보를 설정
        /// </summary>
        /// <param name="duration"> 계산된 방치 시간 </param>
        /// <param name="gold"> 지급될 골드 보상 </param>
        /// <param name="items"> 방치시간동안 획득한 장비 </param><example> 이미 획득한 장비라면 마석으로 자동 분해, count 표시는 방치 시간동안 그냥 몇 개를 획득했는지 보여주기 위한 용도 </example>
        public void Show(TimeSpan duration, long gold, long masuk, Dictionary<EquipmentData, int> items)
        {
            idleDuration = duration;
            rewardedGold = gold;

            durationText.text = $"AFK Time\n{duration.Hours}h {duration.Minutes}m {duration.Seconds}s";
            goldRewardText.text = $"Gold: {BigNumberFormatter.Format(gold)}";
            masukRewardText.text = $"Masuk Change : {BigNumberFormatter.Format(masuk)}";

            if (itemContentParent != null)
            {
                foreach (Transform child in itemContentParent)
                {
                    Destroy(child.gameObject);
                }
            }

            if (items != null && itemContentParent != null && itemSlotPrefab != null)
            {
                foreach (var kvp in items)
                {
                    EquipmentData itemData = kvp.Key;
                    int count = kvp.Value;

                    GameObject slotObj = Instantiate(itemSlotPrefab, itemContentParent);
                    UI_RewardItemSlot slotScript = slotObj.GetComponent<UI_RewardItemSlot>();
                    
                    if (slotScript != null)
                    {
                        slotScript.SetData(itemData, count);
                    }
                }
            }

            gameObject.SetActive(true);
        }

        private void OnConfirmButtonClicked()
        {
            gameObject.SetActive(false);
        }
    }
}