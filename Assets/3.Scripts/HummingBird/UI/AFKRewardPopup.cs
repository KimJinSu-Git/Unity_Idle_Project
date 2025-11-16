using UnityEngine;
using TMPro;
using Bird.Idle.Core;
using Bird.Idle.Data;
using System;
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
        [SerializeField] private Button confirmButton;

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
        /// <param name="duration">계산된 방치 시간</param>
        /// <param name="gold">지급될 골드 보상</param>
        public void Show(TimeSpan duration, long gold)
        {
            idleDuration = duration;
            rewardedGold = gold;

            durationText.text = $"AFK Time:\n{duration.Hours}hour {duration.Minutes}minute {duration.Seconds}second";
            goldRewardText.text = $"Add Gold: {gold.ToString("N0")}";

            gameObject.SetActive(true);
        }

        private void OnConfirmButtonClicked()
        {
            gameObject.SetActive(false);
        }
    }
}