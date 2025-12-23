using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bird.Idle.Gameplay;
using Bird.Idle.Data;
using JetBrains.Annotations;

namespace Bird.Idle.UI
{
    public class MainQuestDisplay : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI questDescriptionText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Button claimButton;
        [CanBeNull] [SerializeField] private GameObject clearEffectObject; // 클리어 가능할 때 이펙트 효과를 넣을까 ?

        private QuestManager questManager;
        private QuestData currentQuest;

        private void Start()
        {
            questManager = QuestManager.Instance;
            if (questManager != null)
            {
                questManager.OnQuestProgressUpdated += UpdateUI;
                questManager.OnMainQuestChanged += UpdateUI;
                
                UpdateUI();
            }
            
            if (claimButton != null)
                claimButton.onClick.AddListener(OnQuestClicked);
        }

        private void OnDestroy()
        {
            if (questManager != null)
            {
                questManager.OnQuestProgressUpdated -= UpdateUI;
                questManager.OnMainQuestChanged -= UpdateUI;
            }
        }

        private void UpdateUI()
        {
            currentQuest = questManager.CurrentMainQuest;

            // 모든 퀘스트 완료 시
            if (currentQuest == null)
            {
                questDescriptionText.text = "All Quest Complete!";
                progressText.text = "";
                if (clearEffectObject != null) clearEffectObject.SetActive(false);
                claimButton.interactable = false;
                return;
            }

            long currentVal = 0;
            var progressObj = questManager.GetQuestProgress(currentQuest.questID); 
            if (progressObj != null) currentVal = progressObj.currentValue;

            long targetVal = currentQuest.targetValue;

            questDescriptionText.text = currentQuest.description;
            
            long displayVal = (currentVal > targetVal) ? targetVal : currentVal;
            
            progressText.text = $"{displayVal} / {targetVal}";

            bool isClearable = currentVal >= targetVal;
            if (clearEffectObject != null) clearEffectObject.SetActive(isClearable);
            
            claimButton.interactable = isClearable;
        }

        private void OnQuestClicked()
        {
            if (currentQuest != null)
            {
                questManager.ClaimReward(currentQuest.questID);
            }
        }
    }
}