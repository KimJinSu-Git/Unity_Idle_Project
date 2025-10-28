using UnityEngine;
using System;
using Bird.Idle.Data; // TODO :: 성장 데이터 연동

namespace Bird.Idle.Core
{
    /// <summary>
    /// 플레이어의 레벨, 스탯, 경험치를 관리하는 싱글톤 클래스
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }

        [Header("Base Stats")]
        [SerializeField] private int characterLevel = 1;
        [SerializeField] private long currentExp = 0;
        [SerializeField] private long requiredExpToNextLevel = 100;
        
        [Header("Battle Stats")]
        [SerializeField] private float baseAttackPower = 10f; // 기본 공격력
        [SerializeField] private float baseMaxHealth = 100f; // 최대 체력

        public int CharacterLevel => characterLevel;
        public float AttackPower => baseAttackPower;
        public float MaxHealth => baseMaxHealth;

        // 레벨 업 이벤트 TODO :: UI 업데이트
        public Action<int> OnLevelUp;

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

        /// <summary>
        /// 경험치를 획득하고 레벨 업을 시도
        /// </summary>
        /// <param name="expAmount">획득할 경험치 양</param>
        public void GainExperience(long expAmount)
        {
            if (expAmount <= 0) return;
            
            currentExp += expAmount;
            Debug.Log($"[CharacterManager] 경험치 획득: +{expAmount}. 현재: {currentExp}");

            CheckForLevelUp();
        }

        /// <summary>
        /// 레벨 업 조건을 검사
        /// </summary>
        private void CheckForLevelUp()
        {
            while (currentExp >= requiredExpToNextLevel)
            {
                characterLevel++;
                
                currentExp -= requiredExpToNextLevel;

                // 다음 레벨 요구 경험치 (임시 : 10%씩 증가)
                requiredExpToNextLevel = (long)(requiredExpToNextLevel * 1.1f);
                
                // 스탯 증가
                baseAttackPower += 2f;
                baseMaxHealth += 20f;

                OnLevelUp?.Invoke(characterLevel);

                Debug.Log($"[CharacterManager] 레벨 업! Lv.{characterLevel}. 다음 EXP: {requiredExpToNextLevel}");
            }
        }
    }
}