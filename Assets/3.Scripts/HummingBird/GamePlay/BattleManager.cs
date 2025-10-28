using UnityEngine;
using Bird.Idle.Core;

namespace Bird.Idle.Gameplay
{
    /// <summary>
    /// 플레이어의 자동 공격 및 전투 상호작용을 관리하는 싱글톤 클래스
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Auto Attack Settings")]
        [SerializeField] private float attackInterval = 0.5f; // 초당 2회 공격
        private float currentAttackCooldown;

        private CharacterManager characterManager;
        private EnemyManager enemyManager;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            characterManager = CharacterManager.Instance;
            enemyManager = EnemyManager.Instance;

            currentAttackCooldown = 0f;
        }

        private void Update()
        {
            currentAttackCooldown -= Time.deltaTime;

            if (currentAttackCooldown <= 0f)
            {
                TryAutoAttack();
                currentAttackCooldown = attackInterval;
            }
        }

        /// <summary>
        /// 몬스터를 자동으로 공격하는 로직을 수행
        /// </summary>
        private void TryAutoAttack()
        {
            if (characterManager == null || enemyManager == null)
            {
                Debug.LogError("[BattleManager] 매니저 참조가 누락되었습니다.");
                return;
            }

            // TODO: 필드의 몬스터 프리팹 리스트를 순회하며 공격하도록 변경(현재는 임시 로직)
            float damage = characterManager.AttackPower;
            
            enemyManager.KillMonster(); 
        }
    }
}