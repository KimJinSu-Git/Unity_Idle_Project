using UnityEngine;
using TMPro;
using Bird.Idle.Gameplay;

namespace Bird.Idle.Visual
{
    public class DamagePopup : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TextMeshPro textMesh;
        
        [Header("Animation Settings")]
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float disappearTimer = 1f;
        [SerializeField] private float fadeOutSpeed = 3f;
        
        [Header("Style Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color criticalColor = Color.yellow;
        [SerializeField] private float normalFontSize = 4f;
        [SerializeField] private float criticalFontSize = 6f;

        private Color textColor;
        private Vector3 moveVector;
        private float timer;
        
        private float currentGameSpeed = 1.0f;

        private void Awake()
        {
            if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        }
        
        private void Start()
        {
            if (BattleManager.Instance != null)
            {
                currentGameSpeed = BattleManager.Instance.GameSpeed;
                BattleManager.Instance.OnGameSpeedChanged += HandleGameSpeedChanged;
            }
        }

        public void Setup(float damageAmount, bool isCritical)
        {
            textMesh.text = damageAmount.ToString("N0");

            if (isCritical)
            {
                textMesh.fontSize = criticalFontSize;
                textColor = criticalColor;
            }
            else
            {
                textMesh.fontSize = normalFontSize;
                textColor = normalColor;
            }

            textMesh.color = textColor;
            timer = disappearTimer;
            
            moveVector = new Vector3(Random.Range(-0.2f, 0.2f), 1f) * moveSpeed;
        }

        private void Update()
        {
            float scaledDeltaTime = Time.deltaTime * currentGameSpeed;
            
            transform.position += moveVector * scaledDeltaTime;
            
            moveVector -= moveVector * (2f * scaledDeltaTime);

            timer -= scaledDeltaTime;
            
            if (timer < 0)
            {
                textColor.a -= fadeOutSpeed * scaledDeltaTime;
                textMesh.color = textColor;

                if (textColor.a < 0)
                {
                    Destroy(gameObject); // TODO ::: 추후 Object Pooling으로 변경 예정
                }
            }
        }
        
        private void HandleGameSpeedChanged(float newSpeed)
        {
            currentGameSpeed = newSpeed;
        }
        
        private void OnDestroy()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnGameSpeedChanged -= HandleGameSpeedChanged;
            }
        }
    }
}