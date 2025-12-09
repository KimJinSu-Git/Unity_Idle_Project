using System;
using System.Collections;
using UnityEngine;
using Bird.Idle.Data;
using Bird.Idle.Core;
using Bird.Idle.Visual;
using UnityEngine.Serialization;

namespace Bird.Idle.Gameplay
{
    /// <summary>
    /// 개별 몬스터 객체의 생명 주기, 스탯, 이동 및 전투 로직을 관리
    /// </summary>
    public class MonsterController : MonoBehaviour, IDamageable
    {
        protected readonly int Attack1Hash = Animator.StringToHash("Attack1");
        
        public MonsterData MonsterData; 
        
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 1.0f;
        
        [Header("Combat")]
        [SerializeField] private float attackInterval = 0.2f;
        
        private float currentHealth;
        private float maxHealth;
        
        private bool isMoving = true;
        private bool currentlyAttacking = false;
        
        protected Animator Animator;
        
        public Action OnHealthChanged;
        
        public bool IsAlive => currentHealth > 0;
        public float AttackRange = 2f; // Player에게 접근해야 하는 거리
        public float GetCurrentHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;
        
        public int InstanceID { get; private set; } 

        /// <summary>
        /// EnemyManager에 의해 스폰될 때 초기화
        /// </summary>
        public void Initialize(MonsterData data, float stageDifficultyMultiplier, int instanceID)
        {
            MonsterData = data;
            InstanceID = instanceID;
            
            maxHealth = MonsterData.baseHealth * stageDifficultyMultiplier;
            currentHealth = maxHealth;
            
            gameObject.name = $"{MonsterData.monsterName}_{InstanceID}";
            
            OnHealthChanged?.Invoke();
            
            Animator = GetComponentInChildren<Animator>();
        }
        
        private void Update()
        {
            if (PlayerController.PlayerTransform == null) return;
            
            Vector3 playerPos = PlayerController.PlayerTransform.position;
            Vector3 targetPosition = new Vector3(playerPos.x, transform.position.y, transform.position.z);
            
            if (isMoving)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }

            if (isMoving)
            {
                float distance = Vector3.Distance(transform.position, targetPosition);
                
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
            
            Animator.Play("Idle");
            
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
        
        protected virtual void TryAttackPlayer()
        {
            float monsterDamage = MonsterData.baseDamage;
            
            if (CharacterManager.Instance != null && monsterDamage > 0)
            {
                CharacterManager.Instance.ApplyDamage(monsterDamage);
                Animator.Play(Attack1Hash);
            }
        }
        
        public void ApplyDamage(float damage)
        {
            if (!IsAlive) return;
            
            currentHealth -= damage;

            OnHealthChanged?.Invoke();
            
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

            if (Animator != null)
            {
                Animator.Play("Death");
            }
            
            Destroy(gameObject, 1f); // 임시적으로, 1초 뒤 사라지도록 함. TODO ::: 오브젝트 풀링으로 바꿉시다.
        }
    }
}