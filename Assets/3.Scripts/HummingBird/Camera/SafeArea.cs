using UnityEngine;

namespace Bird.Idle.UI
{
    public class SafeArea : MonoBehaviour
    {
        private RectTransform panel;
        private Rect lastSafeArea = Rect.zero;

        private void Awake()
        {
            panel = GetComponent<RectTransform>();
            Refresh();
        }

        private void Update()
        {
            if (lastSafeArea != Screen.safeArea)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            lastSafeArea = Screen.safeArea;
            ApplySafeArea(lastSafeArea);
        }

        private void ApplySafeArea(Rect r)
        {
            // 노치 영역만큼 패널의 앵커를 조절해 안쪽으로 밀어넣음
            Vector2 anchorMin = r.position;
            Vector2 anchorMax = r.position + r.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
        }
    }
}