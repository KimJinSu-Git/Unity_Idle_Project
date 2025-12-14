using UnityEngine;
using UnityEngine.UI;

namespace Bird.Idle.UI
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class UI_ResponsiveGrid : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int columnCount = 5; 
        [SerializeField] private float cellAspectRatio = 1.28f;

        private GridLayoutGroup grid;
        private RectTransform rectTransform;

        private void Awake()
        {
            grid = GetComponent<GridLayoutGroup>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            UpdateCellSize();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateCellSize();
        }
        
#if UNITY_EDITOR
        private void Update()
        {
            UpdateCellSize();
        }
#endif

        public void UpdateCellSize()
        {
            if (grid == null || rectTransform == null) return;

            // 현재 패널의 전체 너비
            float totalWidth = rectTransform.rect.width;

            // 패널의 좌우 여백 빼기
            float paddingHorizontal = grid.padding.left + grid.padding.right;

            // 셀 사이 간격빼기
            float spacingTotal = grid.spacing.x * (columnCount - 1);

            // 셀들이 사용할 수 있는 너비 계산
            float availableWidth = totalWidth - paddingHorizontal - spacingTotal;

            // 셀 1개의 너비 결정
            float cellWidth = availableWidth / columnCount;

            // 비율에 맞춰 높이 결정
            float cellHeight = cellWidth / cellAspectRatio;

            // 그리드 적용
            grid.cellSize = new Vector2(cellWidth, cellHeight);
        }
    }
}