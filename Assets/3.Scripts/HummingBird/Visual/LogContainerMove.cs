using System;
using System.Collections;
using Bird.Idle.UI;
using UnityEngine;

namespace Bird.Idle.Visual
{
    public class LogContainerMove : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BottomTabManager bottomTabManager;

        [Header("Settings")] 
        private float truePosY = -235f;
        private float falsePosY = -825f;
        private float lerpSpeed = 10f;

        private RectTransform rectTransform;
        private Coroutine moveCoroutine;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            if (bottomTabManager != null)
            {
                bottomTabManager.OnPanelStateChanged += UpdateTrasformState;
            }
        }
        
        private void OnDestroy()
        {
            if (bottomTabManager != null)
            {
                bottomTabManager.OnPanelStateChanged -= UpdateTrasformState;
            }
        }

        private void UpdateTrasformState(bool isPanelOn)
        {
            float targetPosY = isPanelOn ? truePosY : falsePosY;

            // 이미 이동 중이라면 기존 이동을 취소.
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }

            // 새로운 목표 위치로 이동
            moveCoroutine = StartCoroutine(MoveToYCoroutine(targetPosY));
        }

        private IEnumerator MoveToYCoroutine(float targetPosY)
        {
            // 현재 Y값과 목표 Y 값의 차이가 0.1보다 클 때만 반복
            while (Mathf.Abs(rectTransform.anchoredPosition.y - targetPosY) > 0.1f)
            {
                Vector2 currentPos = rectTransform.anchoredPosition;
                
                // Time.deltaTime을 곱하여 프레임 드랍에 상관없이 일정한 속도를 보장
                currentPos.y = Mathf.Lerp(currentPos.y, targetPosY, lerpSpeed * Time.deltaTime);
                rectTransform.anchoredPosition = currentPos;

                yield return null;
            }
            
            // 미세한 오차를 없애기 위해 목표 위치에 정확히 고정
            Vector2 finalPos = rectTransform.anchoredPosition;
            finalPos.y = targetPosY;
            rectTransform.anchoredPosition = finalPos;

            moveCoroutine = null;
        }
    }
}

