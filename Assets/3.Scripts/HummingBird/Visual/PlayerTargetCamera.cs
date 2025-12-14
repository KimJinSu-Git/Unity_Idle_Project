using UnityEngine;

namespace Bird.Idle.Visual
{
    [RequireComponent(typeof(Camera))]
    public class PlayerTargetCamera : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform playerTransform;
        
        [Range(0f, 1f)]
        [SerializeField] private float targetViewportX = 0.2f;
        
        [Header("Offset Settings")]
        [SerializeField] private float yOffset = 0f;
        [SerializeField] private float smoothSpeed = 5f;

        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (playerTransform == null) return;

            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            float totalWidth = camWidth * 2f;
            
            float targetCamX = playerTransform.position.x - (totalWidth * (targetViewportX - 0.5f));

            Vector3 targetPos = new Vector3(targetCamX, playerTransform.position.y + yOffset, -10f);

            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
        }
    }
}