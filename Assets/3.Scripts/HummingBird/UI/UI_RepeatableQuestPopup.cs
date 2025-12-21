using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Bird.Idle.Data;
using Bird.Idle.Gameplay;

namespace Bird.Idle.UI
{
    public class UI_RepeatableQuestPopup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform contentParent; // ScrollView의 Content
        [SerializeField] private GameObject slotPrefab;   // UI_QuestSlot 프리팹
        [SerializeField] private Button backgroundCloseButton; // 배경 터치 닫기용

        private List<UI_QuestSlot> createdSlots = new List<UI_QuestSlot>();

        private void Awake()
        {
            if (backgroundCloseButton != null) backgroundCloseButton.onClick.AddListener(ClosePopup);
            ClosePopup();
        }

        private void OnEnable()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestProgressUpdated += RefreshUI;
                RefreshUI();
            }
        }

        private void OnDisable()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestProgressUpdated -= RefreshUI;
            }
        }
        
        public void OpenPopup()
        {
            gameObject.SetActive(true);
        }

        private void ClosePopup()
        {
            gameObject.SetActive(false);
        }

        private void RefreshUI()
        {
            List<QuestData> repeatQuests = QuestManager.Instance.GetRepeatableQuests();
            
            while (createdSlots.Count < repeatQuests.Count)
            {
                GameObject obj = Instantiate(slotPrefab, contentParent);
                UI_QuestSlot slot = obj.GetComponent<UI_QuestSlot>();
                if (slot != null) createdSlots.Add(slot);
            }

            for (int i = 0; i < createdSlots.Count; i++)
            {
                if (i < repeatQuests.Count)
                {
                    QuestData data = repeatQuests[i];
                    QuestProgress progress = QuestManager.Instance.GetQuestProgress(data.questID);
                    
                    createdSlots[i].SetData(data, progress);
                    createdSlots[i].gameObject.SetActive(true);
                }
                else
                {
                    createdSlots[i].gameObject.SetActive(false);
                }
            }
        }
    }
}