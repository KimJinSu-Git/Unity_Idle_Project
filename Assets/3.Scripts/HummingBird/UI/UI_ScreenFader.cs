using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Bird.Idle.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UI_ScreenFader : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float defaultDuration = 0.5f;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
        public Coroutine FadeOut(float duration = -1f)
        {
            if (duration < 0) duration = defaultDuration;
            return StartCoroutine(FadeRoutine(0f, 1f, duration));
        }
        public Coroutine FadeIn(float duration = -1f)
        {
            if (duration < 0) duration = defaultDuration;
            return StartCoroutine(FadeRoutine(1f, 0f, duration));
        }

        private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration)
        {
            float elapsed = 0f;
            
            canvasGroup.blocksRaycasts = true;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = endAlpha;

            if (endAlpha == 0f)
            {
                canvasGroup.blocksRaycasts = false;
            }
        }
    }
}