using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bird.Idle.Gameplay;
using Bird.Idle.Data;

namespace Bird.Idle.UI
{
    public class MainQuestDisplay : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI questDescriptionText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Button claimButton; // 전체를 감싸는 투명 버튼(메인 퀘스트 버튼? 느낌)
        [SerializeField] private GameObject clearEffectObject; // 클리어 가능할 때 이펙트 효과를 넣을까 ?

        private QuestManager questManager;
        private QuestData currentQuest;

        private void Start()
        {
            questManager = QuestManager.Instance;
            if (questManager != null)
            {
                questManager.OnQuestProgressUpdated += UpdateUI;
                questManager.OnMainQuestChanged += UpdateUI;
                
                // 최초 1회 갱신
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
                questDescriptionText.text = "모든 메인 퀘스트 완료!";
                progressText.text = "";
                progressSlider.value = 1;
                clearEffectObject.SetActive(false);
                claimButton.interactable = false;
                return;
            }

            // QuestManager에 GetProgress(int id) 메서드를 추가
            long currentVal = 0;
            var progressObj = questManager.GetQuestProgress(currentQuest.questID); 
            if (progressObj != null) currentVal = progressObj.currentValue;

            long targetVal = currentQuest.targetValue;

            questDescriptionText.text = currentQuest.description;
            progressText.text = $"{currentVal} / {targetVal}";

            if (targetVal > 0)
                progressSlider.value = (float)currentVal / targetVal;
            else
                progressSlider.value = 0;

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