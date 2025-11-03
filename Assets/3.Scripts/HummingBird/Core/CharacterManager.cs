using UnityEngine;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Bird.Idle.Data;
using Bird.Idle.Gameplay;

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
        // [SerializeField] private LevelData levelData; 어드레서블로 변경 후 제거 예정(우선 혹시 모를 사태를 대비해 남겨놨음. 추후 제거 예정)
        [SerializeField] private AssetReferenceT<LevelData> levelDataReference;
        
        private LevelData loadedLevelData;
        
        private float permanentAttackBonus = 0f;
        private float permanentHealthBonus = 0f;

        public int CharacterLevel => characterLevel;
        public float AttackPower 
        {
            get 
            {
                float bonus = 0;
                // 장비 보너스 추가
                if (InventoryManager.Instance != null)
                {
                    bonus = InventoryManager.Instance.GetTotalEquipmentBonus().totalAttack;
                }
                return baseAttackPower + permanentAttackBonus + bonus;
            }
        }
        public float MaxHealth 
        {
            get 
            {
                float bonus = 0;
                if (InventoryManager.Instance != null)
                {
                    bonus = InventoryManager.Instance.GetTotalEquipmentBonus().totalHealth;
                }
                return baseAttackPower + permanentHealthBonus + bonus;
            }
        }

        public Action<int> OnLevelUp; // 레벨 업 이벤트 (레벨업 시 스탯 변경 이벤트)
        public Action<long, long> OnExpChanged; // 경험치 변화 이벤트(몬스터를 잡아 경험치를 획득할 때 마다 호출되는 용도)
        public Action OnStatsRecalculated; // 스탯 변경 이벤트(장비 장착/해제 시 UI 업데이트 용도)

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadLevelDataAsync();
        }
        
        /// <summary>
        /// Addressables를 사용하여 LevelData를 로드
        /// </summary>
        private async void LoadLevelDataAsync()
        {
            AsyncOperationHandle<LevelData> handle = levelDataReference.LoadAssetAsync<LevelData>();

            await handle.Task; 
        
            // 로드 성공 시 데이터 캐시
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedLevelData = handle.Result;
                Debug.Log("[CharacterManager] LevelData Addressables 로드 완료!");
            
                OnLevelUp?.Invoke(characterLevel);
                OnExpChanged?.Invoke(currentExp, GetRequiredExpToNextLevel());
            }
            else
            {
                Debug.LogError($"[CharacterManager] LevelData Addressables 로드 실패: {handle.OperationException}");
            }
        
            // TODO: 사용이 끝난 시점에 handle.Release()를 호출하여 메모리를 해제 추가
        }
        
        /// <summary>
        /// 장비 변경이나 강화 시 스탯을 재계산하고 UI 업데이트를 요청
        /// </summary>
        public void ApplyEquipmentStats()
        {
            OnStatsRecalculated?.Invoke(); 
    
            // TODO :: UI 로직: StatsDisplay에서 이 이벤트를 구독하여 AttackText, HealthText를 갱신
            Debug.Log($"[CharacterManager] 장비 스탯 재계산 완료. 최종 ATK: {AttackPower}");
        }
        
        public void ApplyBaseStatUpgrade(float attackIncrease, float healthIncrease)
        {
            permanentAttackBonus += attackIncrease;
            permanentHealthBonus += healthIncrease;
    
            OnStatsRecalculated?.Invoke(); 
    
            Debug.Log($"[CharacterManager] 기본 스탯 영구 증가! ATK: {permanentAttackBonus:F2}");
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
            if (loadedLevelData == null) 
            {
                // 데이터 로드가 완료되지 않았다면 반환
                Debug.LogWarning("[CharacterManager] LevelData 로드 대기 중...");
                return;
            }
            
            LevelData.LevelEntry nextLevelEntry = loadedLevelData.GetLevelEntry(characterLevel + 1);
            
            // 데이터 없으면 최대 레벨
            if (nextLevelEntry.Level == 0) return;
            
            long requiredExp = nextLevelEntry.RequiredExp;
            
            while (currentExp >= requiredExp)
            {
                characterLevel++;
                currentExp -= requiredExp;

                // 다음 레벨 요구 경험치
                LevelData.LevelEntry newEntry = loadedLevelData.GetLevelEntry(characterLevel);
                LevelData.LevelEntry newNextLevelEntry = loadedLevelData.GetLevelEntry(characterLevel + 1);
                
                // 스탯 증가
                baseAttackPower += newEntry.AttackIncrease;
                baseMaxHealth += newEntry.HealthIncrease;

                OnLevelUp?.Invoke(characterLevel);
                
                OnExpChanged?.Invoke(currentExp, newNextLevelEntry.RequiredExp);

                Debug.Log($"[CharacterManager] 레벨 업! Lv.{characterLevel}. 다음 EXP: {newNextLevelEntry.RequiredExp}");
            }
        }
        
        private long GetTotalExpForLevel(int level)
        {
            if (loadedLevelData == null) return 0;
            return loadedLevelData.GetLevelEntry(level).RequiredExp;
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