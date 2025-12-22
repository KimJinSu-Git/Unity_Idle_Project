using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bird.Idle.Gameplay;
using Bird.Idle.Core;
using JetBrains.Annotations;

namespace Bird.Idle.UI
{
    public class UI_GameSpeedButton : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI speedText;
        [CanBeNull] [SerializeField] private Image iconImage;
        
        [Header("Icons/Sprites (Optional)")]
        [CanBeNull] [SerializeField] private Sprite x1Sprite;
        [CanBeNull] [SerializeField] private Sprite x1_5Sprite;
        [CanBeNull] [SerializeField] private Sprite x2Sprite;

        private float maxUnlockedSpeed = 1.0f;

        private void Awake()
        {
            if (button != null) button.onClick.AddListener(OnClickSpeedButton);
        }

        private void Start()
        {
            if (BattleManager.Instance != null)
            {
                UpdateUI(BattleManager.Instance.GameSpeed);
            }

            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.OnGameSpeedUnlock += UpdateUnlockStatus;
                
                CheckInitialLevel(CharacterManager.Instance.CharacterLevel);
            }
        }
        
        private void OnDestroy()
        {
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.OnGameSpeedUnlock -= UpdateUnlockStatus;
            }
        }

        private void CheckInitialLevel(int level)
        {
            float unlocked = 1.0f;
            if (level >= 50) unlocked = 2.0f;
            else if (level >= 20) unlocked = 1.5f;
            
            UpdateUnlockStatus(unlocked);
        }

        private void UpdateUnlockStatus(float unlockedSpeed)
        {
            maxUnlockedSpeed = unlockedSpeed;
        }

        private void OnClickSpeedButton()
        {
            float currentSpeed = BattleManager.Instance.GameSpeed;
            float nextSpeed = 1.0f;

            if (Mathf.Approximately(currentSpeed, 1.0f)) nextSpeed = 1.5f;
            else if (Mathf.Approximately(currentSpeed, 1.5f)) nextSpeed = 2.0f;
            else nextSpeed = 1.0f;

            if (nextSpeed > maxUnlockedSpeed)
            {
                string msg = "";
                if (nextSpeed == 1.5f) msg = "Lv.5 Reach Please !";
                else if (nextSpeed == 2.0f) msg = "Lv.10 Reach Please !";
                
                if (UI_ToastMessage.Instance != null)
                    UI_ToastMessage.Instance.Show(msg);
                else
                    Debug.Log($"[UI] Locked: {msg}");
                    
                return;
            }

            BattleManager.Instance.SetGameSpeed(nextSpeed);
            UpdateUI(nextSpeed);
        }

        private void UpdateUI(float speed)
        {
            if (speedText != null)
            {
                speedText.text = $"x{speed:0.0}";
            }

            if (iconImage != null)
            {
                if (Mathf.Approximately(speed, 1.0f)) iconImage.sprite = x1Sprite;
                else if (Mathf.Approximately(speed, 1.5f)) iconImage.sprite = x1_5Sprite;
                else if (Mathf.Approximately(speed, 2.0f)) iconImage.sprite = x2Sprite;
            }
        }
    }
}