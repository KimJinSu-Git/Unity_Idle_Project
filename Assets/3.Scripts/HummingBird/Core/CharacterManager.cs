using UnityEngine;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Bird.Idle.Data;
using Bird.Idle.Gameplay;
using Bird.Idle.Visual;

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
        private Vector3 spawnPosition;
        
        public StatComponent PlayerStats { get; private set; } = new StatComponent();
        
        public float AttackPower => PlayerStats.FinalAttackPower;
        public float MaxHealth => PlayerStats.FinalMaxHealth;
        public float FinalHealthRegen => PlayerStats.FinalHealthRegen;
        public long CurrentEXP => currentEXP;
        public int AvailableStatPoints => availableStatPoints;
        public int Strength => PlayerStats.Strength;
        public int Dexterity => PlayerStats.Dexterity;
        public int Intelligence => PlayerStats.Intelligence;
        public int Luck => PlayerStats.Luck;
        public float GetCurrentHealth => currentHealth;
        public int CharacterLevel => characterLevel;
        public float PlayerAttackRange => playerAttackRange;
        public bool IsAlive => currentHealth > 0;

        public Action<int> OnLevelUp; // 레벨 업 이벤트 (레벨업 시 스탯 변경 이벤트)
        public Action OnStatsRecalculated; // 스탯 변경 이벤트(장비 장착/해제 시 UI 업데이트 용도)
        public Action OnPlayerDied; // 플레이어 사망 이벤트
        public Action OnHealthChanged; // 체력 변경 이벤트 (UI 갱신용)
        public Action OnEXPChanged; // 경험치 변경 이벤트
        public Action OnRequestStageRestart; // 재시작 요청 이벤트
        public Action OnPlayerRevived; // Player 부활 이벤트

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

        private void Start()
        {
            spawnPosition = PlayerController.PlayerTransform.position - new Vector3(0.25f, 0f, 0f);
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
            OnHealthChanged?.Invoke();
            OnStatsRecalculated?.Invoke();
        }
        
        public void GainExperience(long expAmount)
        {
            currentEXP += expAmount
                ;
            OnEXPChanged?.Invoke();
            
            while (CheckForLevelUp())
            {
                long requiredExp = GetRequiredEXP(characterLevel);
        
                currentEXP -= requiredExp;
                
                characterLevel++;
                availableStatPoints++; // 레벨업 당 1포인트 지급
                
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
            
            return nextEntry.RequiredEXP;
        }
        
        
        
        public void ApplyDamage(float damage)
        {
            if (!IsAlive) return;

            currentHealth -= damage;
            
            DamagePopupManager.Instance.CreatePopup(spawnPosition, damage, false);
            
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
            
            OnRequestStageRestart?.Invoke();
        }
        
        public void Revive()
        {
            currentHealth = MaxHealth;
            OnHealthChanged?.Invoke();
            OnPlayerRevived?.Invoke();
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
            return false;
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
            
            OnHealthChanged?.Invoke(); 
            OnStatsRecalculated?.Invoke();
        }
        
        public void ApplyBaseStatUpgrade(float attackIncrease, float healthIncrease)
        {
            permanentAttackBonus += attackIncrease;
            permanentHealthBonus += healthIncrease;
    
            OnStatsRecalculated?.Invoke(); 
        }
    }
}