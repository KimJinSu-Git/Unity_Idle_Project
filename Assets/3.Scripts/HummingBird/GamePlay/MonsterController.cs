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
        
        private float currentHealth;
        
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