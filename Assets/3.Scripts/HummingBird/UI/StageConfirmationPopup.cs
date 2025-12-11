using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Bird.Idle.Gameplay;

namespace Bird.Idle.UI
{
    public class StageConfirmationPopup : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private int targetStageID;
        private Action onConfirmCallback;

        private void Awake()
        {
            yesButton.onClick.AddListener(OnYesClicked);
            noButton.onClick.AddListener(Close);
        }

        public void Show(int stageID, string stageName, Action onConfirm)
        {
            targetStageID = stageID;
            onConfirmCallback = onConfirm;

            messageText.text = $"[{stageName}]\n Stage Move ? ";
            
            gameObject.SetActive(true);
        }

        private void OnYesClicked()
        {
            onConfirmCallback?.Invoke();
            Close();
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }
    }
}