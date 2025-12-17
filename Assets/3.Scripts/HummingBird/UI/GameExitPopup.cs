using UnityEngine;
using UnityEngine.UI;
using Bird.Idle.Core;

namespace Bird.Idle.UI
{
    public class GameExitPopup : MonoBehaviour
    {
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private void Awake()
        {
            yesButton.onClick.AddListener(OnYesClicked);
            noButton.onClick.AddListener(OnNoClicked);
        }

        private async void OnYesClicked()
        {
            await GameManager.Instance.SaveGameOnExitAsync();

            Application.Quit();
        }

        private void OnNoClicked()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}