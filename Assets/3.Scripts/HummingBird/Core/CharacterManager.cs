using UnityEngine;
using System;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Bird.Idle.Data;
using Bird.Idle.Gameplay;
using UnityEngine.Serialization;

namespace Bird.Idle.Core
{
    /// <summary>
    /// 플레이어의 레벨, 스탯, 경험치를 관리하는 싱글톤 클래스
    /// </summary>
    public class CharacterManager : MonoBehaviour, IDamageable
    {
        public static CharacterManager Instance { get; private set; }

        [Header("Base Stats")]
        [SerializeField] private int characterLevel = 1;
        
        [Header("Battle Stats")]
        [SerializeField] private float playerAttackRange = 1.0f;
        
        [Header("Data References")]
        [SerializeField] private AssetReferenceT<LevelUpCostData> levelUpCostDataReference;
        
        [Header("Stats (STR, DEX, INT, LCK)")]
        private int strength = 1;
        private int dexterity = 1;
        private int intelligence = 1;
        private int luck = 1;
        
        private long currentEXP = 0;
        private int availableStatPoints = 0;
        
        private float currentHealth;
        private float permanentAttackBonus = 0f;
        private float permanentHealthBonus = 0f;
        
        private LevelUpCostData loadedLevelUpCostData;
        
        public StatComponent PlayerStats { get; private set; } = new StatComponent();
        
        public float AttackPower => PlayerStats.FinalAttackPower;
        public float MaxHealth => PlayerStats.FinalMaxHealth;
        public float FinalHealthRegen => PlayerStats.FinalHealthRegen;
        public long CurrentEXP => currentEXP;
        public int AvailableStatPoints => availableStatPoints;
        public int Strength => strength;
        public int Dexterity => dexterity;
        public int Intelligence => intelligence;
        public int Luck => luck;
        public float GetCurrentHealth => currentHealth;
        public int CharacterLevel => characterLevel;
        public float PlayerAttackRange => playerAttackRange;
        public bool IsAlive => currentHealth > 0;
        // public float AttackPower 
        // {
        //     get 
        //     {
        //         // STR 당 2.5 ATK
        //         float baseAtk = 7.5f + (float)strength * 2.5f;
        //         
        //         float equipmentBonus = 0;
        //         if (InventoryManager.Instance != null)
        //         {
        //             equipmentBonus = InventoryManager.Instance.GetTotalEquipmentBonus().totalAttack;
        //         }
        //
        //         return baseAtk + permanentAttackBonus + equipmentBonus;
        //     }
        // }
        // public float MaxHealth 
        // {
        //     get 
        //     {
        //         float baseHp = 95f + (strength * 5f);
        //         
        //         float equipmentBonus = 0;
        //         if (InventoryManager.Instance != null)
        //         {
        //             equipmentBonus = InventoryManager.Instance.GetTotalEquipmentBonus().totalHealth;
        //         }
        //         
        //         // baseHp + 영구보너스 + 장비보너스
        //         return baseHp + permanentHealthBonus + equipmentBonus;
        //     }
        // }

        public Action<int> OnLevelUp; // 레벨 업 이벤트 (레벨업 시 스탯 변경 이벤트)
        public Action OnStatsRecalculated; // 스탯 변경 이벤트(장비 장착/해제 시 UI 업데이트 용도)
        public Action OnPlayerDied; // 플레이어 사망 이벤트
        public Action OnHealthChanged; // 체력 변경 이벤트 (UI 갱신용)

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            currentHealth = MaxHealth;
            
            LoadLevelUpCostDataAsync();
        }
        
        /// <summary>
        /// GameManager에서 로드된 데이터를 받아 캐릭터 상태를 초기화
        /// </summary>
        public void Initialize(GameSaveData data)
        {
            // 레벨 복원
            characterLevel = data.PlayerLevel;
            
            // 영구 보너스 스탯 복원 (Slot 강화 등으로 얻은 스탯)
            permanentAttackBonus = data.PermanentAttackBonus;
            permanentHealthBonus = data.PermanentHealthBonus;
            
            currentEXP = data.CurrentEXP;
            availableStatPoints = data.AvailableStatPoints;
            strength = data.Strength;
            dexterity = data.Dexterity;
            intelligence = data.Intelligence;
            luck = data.Luck;
            
            PlayerStats.RestoreCoreStats(data.Strength, data.Dexterity, data.Intelligence, data.Luck);
            
            RecalculateAllFinalStats();
            currentHealth = MaxHealth;
            
            Debug.Log($"[CharacterManager] 캐릭터 데이터 로드 완료.");
            
            // UI 갱신
            OnLevelUp?.Invoke(characterLevel);
            OnStatsRecalculated?.Invoke();
        }
        
        public void GainExperience(long expAmount)
        {
            currentEXP += expAmount;
            
            while (CheckForLevelUp())
            {
                characterLevel++;
                availableStatPoints++; // 레벨업 당 1포인트 지급
                // TODO ::: 레벨업 시 경험치 필요량 차감 로직 추가
                
                OnLevelUp?.Invoke(characterLevel);
                Debug.Log($"[CharacterManager] 레벨 업! Lv.{characterLevel}. 스탯 포인트 획득.");
            }
        }
        
        private bool CheckForLevelUp()
        {
            if (loadedLevelUpCostData == null) return false;
            
            long requiredExp = GetRequiredEXP(characterLevel);
            
            return requiredExp != -1 && currentEXP >= requiredExp;
        }
        
        public long GetRequiredEXP(int level)
        {
            if (loadedLevelUpCostData == null) return -1;
            
            LevelUpCostData.LevelEntry nextEntry = loadedLevelUpCostData.GetLevelEntry(level + 1);
            
            if (nextEntry.Level == 0) return -1;
            
            return nextEntry.RequiredGold; // TODO ::: 임시로 RequiredGold 필드를 EXP로 사용 나중에 RequiredEXP 추가해야함.
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
            Debug.Log("[CharacterManager] 플레이어가 사망했습니다!");
            OnPlayerDied?.Invoke();
            
            // TODO: 스테이지 재시작 로직 호출
        }
        
        /// <summary>
        /// DataManager에 저장할 캐릭터 데이터를 GameSaveData 형식으로 수집하여 반환
        /// </summary>
        public void CollectSaveData(GameSaveData data)
        {
            data.PlayerLevel = characterLevel;
            data.PermanentAttackBonus = permanentAttackBonus;
            data.PermanentHealthBonus = permanentHealthBonus;
            
            data.CurrentEXP = currentEXP;
            data.AvailableStatPoints = availableStatPoints;
            data.Strength = PlayerStats.Strength;
            data.Dexterity = PlayerStats.Dexterity;
            data.Intelligence = PlayerStats.Intelligence;
            data.Luck = PlayerStats.Luck;
        }
        
        public bool TryAllocateStatPoint(string statName)
        {
            if (availableStatPoints <= 0) return false;
            
            if (PlayerStats.TryAllocateStatPoint(statName))
            {
                availableStatPoints--;
                RecalculateAllFinalStats();
                return true;
            }
            return true;
        }
        
        /// <summary>
        /// Addressables를 사용하여 LevelData를 로드
        /// </summary>
        private async void LoadLevelUpCostDataAsync()
        {
            AsyncOperationHandle<LevelUpCostData> handle = levelUpCostDataReference.LoadAssetAsync<LevelUpCostData>();

            await handle.Task; 
        
            // 로드 성공 시 데이터 캐시
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedLevelUpCostData = handle.Result;
                Debug.Log("[CharacterManager] LevelUpCostData Addressables 로드 완료!");
            
                OnLevelUp?.Invoke(characterLevel);
            }
            else
            {
                Debug.LogError($"[CharacterManager] LevelUpCostData Addressables 로드 실패: {handle.OperationException}");
            }
        
            // TODO: 사용이 끝난 시점에 handle.Release()를 호출하여 메모리를 해제 추가
        }
        
        public void RecalculateAllFinalStats()
        {
            var equipBonus = InventoryManager.Instance.GetTotalEquipmentBonusExtended();

            PlayerStats.CalculateFinalStats(equipBonus, permanentAttackBonus, permanentHealthBonus);
            
            OnStatsRecalculated?.Invoke();
        }
        
        /// <summary>
        /// 장비 변경이나 강화 시 스탯을 재계산하고 UI 업데이트를 요청
        /// </summary>
        public void ApplyEquipmentStats()
        {
            RecalculateAllFinalStats();
        }
        
        public void ApplyBaseStatUpgrade(float attackIncrease, float healthIncrease)
        {
            permanentAttackBonus += attackIncrease;
            permanentHealthBonus += healthIncrease;
    
            OnStatsRecalculated?.Invoke(); 
        }
        
        // /// <summary>
        // /// 골드를 소모하여 플레이어 레벨업을 시도
        // /// </summary>
        // public bool TryLevelUp()
        // {
        //     if (loadedLevelUpCostData == null) return false;
        //     
        //     LevelUpCostData.LevelEntry nextLevelEntry = loadedLevelUpCostData.GetLevelEntry(characterLevel + 1);
        //
        //     if (nextLevelEntry.Level == 0)
        //     {
        //         Debug.Log("[CharacterManager] 이미 최대 레벨입니다.");
        //         return false;
        //     }
        //
        //     long goldCost = nextLevelEntry.RequiredGold;
        //
        //     if (CurrencyManager.Instance == null || !CurrencyManager.Instance.CanAfford(CurrencyType.Gold, goldCost))
        //     {
        //         Debug.LogWarning($"[CharacterManager] 레벨업 골드 부족. 필요: {goldCost:N0}");
        //         return false;
        //     }
        //
        //     CurrencyManager.Instance.ChangeCurrency(CurrencyType.Gold, -goldCost);
        //     characterLevel++;
        //
        //     OnLevelUp?.Invoke(characterLevel);
        //
        //     Debug.Log($"[CharacterManager] 레벨 업! Lv.{characterLevel}.");
        //     return true;
        // }
        
        // public long GetLevelUpCost(int currentLevel)
        // {
        //     if (loadedLevelUpCostData == null) return 0;
        //
        //     LevelUpCostData.LevelEntry nextEntry = loadedLevelUpCostData.GetLevelEntry(currentLevel + 1);
        //     
        //     if (nextEntry.Level == 0) return -1;
        //     
        //     return nextEntry.RequiredGold;
        // }
    }
}