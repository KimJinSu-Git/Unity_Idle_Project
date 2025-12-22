using UnityEngine;
using TMPro;
using System.Collections;

namespace Bird.Idle.UI
{
    public class UI_ToastMessage : MonoBehaviour
    {
        public static UI_ToastMessage Instance { get; private set; }

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float displayDuration = 1.5f;

        private void Awake()
        {
            Instance = this;
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        public void Show(string message)
        {
            gameObject.SetActive(true);
            messageText.text = message;
            
            StopAllCoroutines();
            StartCoroutine(FadeRoutine());
        }

        private IEnumerator FadeRoutine()
        {
            float timer = 0f;
            while(timer < 0.2f)
            {
                timer += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / 0.2f);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(displayDuration);

            timer = 0f;
            while (timer < 0.5f)
            {
                timer += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / 0.5f);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}