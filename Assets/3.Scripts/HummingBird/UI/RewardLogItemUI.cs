using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace Bird.Idle.UI
{
    public class RewardLogItemUI : MonoBehaviour
    {
        [Header("UI Elements")] 
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI logText;
        [SerializeField] private CanvasGroup canvasGroup; // 투명도 조절

        [Header("Settings")] 
        [SerializeField] private float displayTime = 5.5f; // 표시 유지 시간
        [SerializeField] private float fadeTime = 0.5f; // 사라지는 시간
        
        private Coroutine fadeCoroutine;

        public void Setup(Sprite icon, string message, Color textColor)
        {
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }

            logText.text = message;
            logText.color = textColor;
            
            // 투명도 초기화
            canvasGroup.alpha = 1f;
            
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            
            fadeCoroutine = StartCoroutine(FadeOutAndReturn());
        }

        // 로그 최대 갯수(6개) 초과 시 매니저가 호출할 강제 종료
        public void ForceClose()
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            RewardLogManager.Instance.ReturnToPool(this);
        }

        private IEnumerator FadeOutAndReturn()
        {
            yield return new WaitForSeconds(displayTime);

            // 투명해짐
            float t = 0;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
                yield return null;
            }

            RewardLogManager.Instance.ReturnToPool(this);
        }
    }

}
