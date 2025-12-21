using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bird.Idle.Data;
using Bird.Idle.Gameplay;
using Bird.Idle.Core;

namespace Bird.Idle.UI
{
    public class UI_QuestSlot : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private Button claimButton;

        private QuestData _data;
        
        private void Awake()
        {
            if (claimButton != null)
                claimButton.onClick.AddListener(OnClaimClicked);
        }

        public void SetData(QuestData data, QuestProgress progress)
        {
            _data = data;

            descriptionText.text = data.description;

            long displayValue = progress.currentValue; 
            
            progressText.text = $"{displayValue} / {data.targetValue}";
            
            if (data.targetValue > 0)
                progressSlider.value = (float)displayValue / data.targetValue;
            else
                progressSlider.value = 0;

            rewardText.text = $"{data.rewardAmount:N0}";

            int stackCount = progress.rewardsClaimed;
            bool canClaim = stackCount > 0;

            claimButton.interactable = canClaim;
        }

        private void OnClaimClicked()
        {
            if (_data != null)
            {
                QuestManager.Instance.ClaimReward(_data.questID);
            }
        }
    }
}