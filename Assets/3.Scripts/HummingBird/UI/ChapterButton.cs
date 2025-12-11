using UnityEngine;
using UnityEngine.UI;
using Bird.Idle.Gameplay;

namespace Bird.Idle.UI
{
    public class ChapterButton : MonoBehaviour
    {
        [Header("Settings")]
        public int ChapterID;
        public string ChapterName;

        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private GameObject lockIcon; // TODO :: 나중에 자물쇠 아이콘 가져오자

        private void Start()
        {
            if (button == null) button = GetComponent<Button>();
            button.onClick.AddListener(OnChapterClicked);
        }

        private void OnEnable()
        {
            UpdateChapterState();
        }

        public void UpdateChapterState()
        {
            if (StageManager.Instance == null) return;

            int unlockConditionStage = (ChapterID - 1) * 10; 
            
            bool isUnlocked = StageManager.Instance.MaxReachedStageID > unlockConditionStage;
            
            if (ChapterID == 1) isUnlocked = true;

            button.interactable = isUnlocked;
            if (lockIcon != null) lockIcon.SetActive(!isUnlocked);
        }

        private void OnChapterClicked()
        {
            MapUIManager.Instance.OpenStageList(ChapterID, ChapterName);
        }
    }
}