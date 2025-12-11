using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bird.Idle.Gameplay;

namespace Bird.Idle.UI
{
    public class StageNode : MonoBehaviour
    {
        private int stageID;
        private string stageName;

        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI stageNameText;
        [SerializeField] private GameObject lockIcon; // 잠김 표시 마찬가지

        public void Setup(int id, string name)
        {
            stageID = id;
            stageName = name;
            
            if (stageNameText != null) stageNameText.text = stageName;
            
            UpdateState();
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }

        private void UpdateState()
        {
            if (StageManager.Instance == null) return;

            int maxReached = StageManager.Instance.MaxReachedStageID;
            
            bool isUnlocked = stageID <= maxReached;
            bool isCleared = stageID < maxReached; 

            button.interactable = isUnlocked;
            
            if (lockIcon != null) lockIcon.SetActive(!isUnlocked);
        }

        private void OnClicked()
        {
            MapUIManager.Instance.ShowMoveConfirmation(stageID, stageName);
        }
    }
}