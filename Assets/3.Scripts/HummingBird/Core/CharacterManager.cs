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
        // [SerializeField] private long requiredExpToNextLevel = 100;
        
        [Header("Battle Stats")]
        [SerializeField] private float baseAttackPower = 10f; // 기본 공격력
        [SerializeField] private float baseMaxHealth = 100f; // 최대 체력
        
        [Header("Data References")]
        [SerializeField] private LevelData levelData;

        public int CharacterLevel => characterLevel;
        public float AttackPower => baseAttackPower;
        public float MaxHealth => baseMaxHealth;

        // 레벨 업 이벤트 TODO :: UI 업데이트
        public Action<int> OnLevelUp;
        public Action<long, long> OnExpChanged;

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
            
            OnExpChanged?.Invoke(currentExp, GetRequiredExpToNextLevel());

            CheckForLevelUp();
        }

        /// <summary>
        /// 레벨 업 조건을 검사
        /// </summary>
        private void CheckForLevelUp()
        {
            LevelData.LevelEntry nextLevelEntry = levelData.GetLevelEntry(characterLevel + 1);
            
            // 데이터 없으면 최대 레벨
            if (nextLevelEntry.Level == 0) return;
            
            long requiredExp = nextLevelEntry.RequiredExp;
            
            while (currentExp >= requiredExp)
            {
                characterLevel++;
                currentExp -= requiredExp;

                // 다음 레벨 요구 경험치
                LevelData.LevelEntry newEntry = levelData.GetLevelEntry(characterLevel);
                LevelData.LevelEntry newNextLevelEntry = levelData.GetLevelEntry(characterLevel + 1);
                
                // 스탯 증가
                baseAttackPower += newEntry.AttackIncrease;
                baseMaxHealth += newEntry.HealthIncrease;

                OnLevelUp?.Invoke(characterLevel);
                
                OnExpChanged?.Invoke(currentExp, newNextLevelEntry.RequiredExp);

                Debug.Log($"[CharacterManager] 레벨 업! Lv.{characterLevel}. 다음 EXP: {newNextLevelEntry.RequiredExp}");
            }
        }
        
        // TODO: 현재 레벨까지의 총 경험치 계산 메서드
        private long GetTotalExpForLevel(int level)
        {
            return levelData.GetLevelEntry(level).RequiredExp;
        }
        
        /// <summary>
        /// 현재 누적 경험치량을 반환
        /// </summary>
        public long GetCurrentExp() => currentExp;

        /// <summary>
        /// 다음 레벨까지 필요한 경험치량을 반환
        /// </summary>
        public long GetRequiredExpToNextLevel()
        {
            long totalExpNext = GetTotalExpForLevel(characterLevel + 1);
            return totalExpNext;
        }
    }
}