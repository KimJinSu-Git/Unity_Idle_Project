using UnityEngine;
using Bird.Idle.Visual;

namespace Bird.Idle.Gameplay
{
    public class DamagePopupManager : MonoBehaviour
    {
        public static DamagePopupManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private DamagePopup popupPrefab;
        
        [Header("Spawn Settings")]
        [SerializeField] private Transform popupParent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void CreatePopup(Vector3 position, float damage, bool isCritical)
        {
            Vector3 spawnPos = position + new Vector3(Random.Range(-0.1f, 0.1f), 0.1f, 0f);
            
            DamagePopup popup = Instantiate(popupPrefab, spawnPos, Quaternion.identity, popupParent);
            popup.Setup(damage, isCritical);
        }
    }
}