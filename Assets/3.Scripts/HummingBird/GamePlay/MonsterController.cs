using System.Collections;
using UnityEngine;
using Bird.Idle.Data;
using Bird.Idle.Core;
using UnityEngine.Serialization;

namespace Bird.Idle.Gameplay
{
    /// <summary>
    /// 개별 몬스터 객체의 생명 주기, 스탯, 이동 및 전투 로직을 관리
    /// </summary>
    public class MonsterController : MonoBehaviour, IDamageable
    {
        // 몬스터 프리팹에 인스펙터로 할당됩니다.
        public MonsterData MonsterData; 
        
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 1.0f;
        
        [Header("Combat")]
        [SerializeField] private float attackInterval = 0.2f;
        
        private float currentHealth;
        
        private bool isMoving = true;
        private bool currentlyAttacking = false;
        
        public bool IsAlive => currentHealth > 0;
        public float AttackRange = 2f; // Player에게 접근해야 하는 거리
        
        public int InstanceID { get; private set; } 

        /// <summary>
        /// EnemyManager에 의해 스폰될 때 초기화
        /// </summary>
        public void Initialize(MonsterData data, float stageDifficultyMultiplier, int instanceID)
        {
            MonsterData = data;
            InstanceID = instanceID;
            
            currentHealth = MonsterData.baseHealth * stageDifficultyMultiplier;
            
            gameObject.name = $"{MonsterData.monsterName}_{InstanceID}";
        }
        
        private void Update()
        {
            if (isMoving)
            {
                Vector3 targetPosition = Vector3.zero;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }

            if (isMoving)
            {
                float distance = Vector3.Distance(transform.position, Vector3.zero);
                
                if (distance <= AttackRange)
                {
                    EnterCombatState();
                }
            }
        }
        
        private void EnterCombatState()
        {
            if (currentlyAttacking) return;
            
            isMoving = false;
            currentlyAttacking = true;
            
            // TODO: 이동 애니메이션 멈춤
            // TODO: 공격 애니메이션 시작
            
            StartCoroutine(AttackLoop());
        }
        
        private IEnumerator AttackLoop()
        {
            while (IsAlive)
            {
                yield return new WaitForSeconds(attackInterval); 
                
                if (IsAlive && CharacterManager.Instance.IsAlive)
                {
                    TryAttackPlayer();
                }
            }
        }
        
        private void TryAttackPlayer()
        {
            float monsterDamage = MonsterData.baseDamage;
            Debug.Log($"[MonsterController] {monsterDamage}를 Player에게 입혔습니다.");
            
            if (CharacterManager.Instance != null && monsterDamage > 0)
            {
                CharacterManager.Instance.ApplyDamage(monsterDamage);
                // TODO: 공격 애니메이션 트리거
            }
        }
        
        public void ApplyDamage(float damage)
        {
            if (!IsAlive) return;
            
            currentHealth -= damage;
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        private void Die()
        {
            currentHealth = 0;
            Debug.Log($"[MonsterController] {MonsterData.monsterName} 처치됨.");
            
            EnemyManager.Instance.ProcessMonsterDefeat(MonsterData); 
            
            Destroy(gameObject);
        }

        // TODO: Update에 Player를 향해 이동하는 로직 및 애니메이션 추가
    }
}