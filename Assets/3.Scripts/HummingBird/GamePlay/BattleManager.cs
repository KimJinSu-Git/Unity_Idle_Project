using System;
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

        [Header("Player Attack Settings")]
        [SerializeField] private float baseAttackInterval = 3f;
        private float currentAttackCooldown;

        private CharacterManager characterManager;
        private EnemyManager enemyManager;
        
        private bool playerBattleMode = true;
        
        public Action<bool> OnBattleStateChanged;

        public bool PlayerBattleMode => playerBattleMode;
        
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
        
        /// <summary>
        /// 전투 상태를 설정
        /// </summary>
        public void SetBattleActive(bool active)
        {
            if (playerBattleMode == active) return;
            
            playerBattleMode = active;
            OnBattleStateChanged?.Invoke(active); 
        }

        /// <summary>
        /// 몬스터를 자동으로 공격하는 로직을 수행
        /// </summary>
        public void TryAutoAttack()
        {
            if (characterManager == null || enemyManager == null)
            {
                Debug.LogError("[BattleManager] 매니저 참조가 누락되었습니다.");
                return;
            }

            float damage = characterManager.AttackPower;
            enemyManager.ApplyDamageToCurrentMonster(damage);
        }
        
        public float GetAttackInterval()
        {
            float attackSpeedMultiplier = CharacterManager.Instance.PlayerStats.FinalAttackSpeed;
            
            return baseAttackInterval / attackSpeedMultiplier; 
        }
    }
}