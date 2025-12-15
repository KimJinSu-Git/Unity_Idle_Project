using Bird.Idle.UI;
using UnityEngine;

namespace Bird.Idle.Visual
{
    [RequireComponent(typeof(Camera))]
    public class PlayerTargetCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BottomTabManager bottomTabManager;
        
        [Header("Target Settings")]
        [SerializeField] private Transform playerTransform;
        
        [Range(0f, 1f)]
        [SerializeField] private float targetViewportX = 0.2f;
        
        [Header("Offset Settings")]
        [Tooltip("하단 패널이 열려있을 때의 Y 오프셋")] [SerializeField] private float panelOpenYOffset = 0f;
        [Tooltip("하단 패널이 닫혀있을 때의 Y 오프셋")] [SerializeField] private float panelClosedYOffset = 2.7f;
        [SerializeField] private float smoothSpeed = 7f;

        private Camera cam;
        
        private float currentTargetYOffset;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }
        
        private void Start()
        {
            currentTargetYOffset = panelOpenYOffset;

            if (bottomTabManager != null)
            {
                bottomTabManager.OnPanelStateChanged += UpdateOffsetState;
            }
        }
        
        private void OnDestroy()
        {
            if (bottomTabManager != null)
            {
                bottomTabManager.OnPanelStateChanged -= UpdateOffsetState;
            }
        }
        
        private void UpdateOffsetState(bool isPanelOpen)
        {
            if (isPanelOpen)
            {
                currentTargetYOffset = panelOpenYOffset;
            }
            else
            {
                currentTargetYOffset = panelClosedYOffset;
            }
        }

        private void LateUpdate()
        {
            if (playerTransform == null) return;

            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;
            float totalWidth = camWidth * 2f;
            
            float targetCamX = playerTransform.position.x - (totalWidth * (targetViewportX - 0.5f));

            Vector3 targetPos = new Vector3(targetCamX, playerTransform.position.y + currentTargetYOffset, -10f);

            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
        }
    }
}