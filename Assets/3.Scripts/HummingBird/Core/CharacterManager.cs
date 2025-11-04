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
        
        [Header("Battle Stats")]
        [SerializeField] private float baseAttackPower = 10f; // 기본 공격력
        [SerializeField] private float baseMaxHealth = 100f; // 최대 체력
        
        [Header("Data References")]
        [SerializeField] private AssetReferenceT<LevelData> levelDataReference;
        
        [Header("Level Up Data")]
        private const long BASE_LEVELUP_COST = 1000;
        private const long COST_MULTIPLIER = 120;
        
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
        /// 골드를 소모하여 플레이어 레벨업을 시도
        /// </summary>
        public bool TryLevelUp()
        {
            LevelData.LevelEntry nextLevelEntry = loadedLevelData.GetLevelEntry(characterLevel + 1);

            if (nextLevelEntry.Level == 0)
            {
                Debug.Log("[CharacterManager] 이미 최대 레벨입니다.");
                return false;
            }

            long goldCost = BASE_LEVELUP_COST + (long)characterLevel * characterLevel * COST_MULTIPLIER;
        
            if (CurrencyManager.Instance == null || !CurrencyManager.Instance.CanAfford(CurrencyType.Gold, goldCost))
            {
                Debug.LogWarning($"[CharacterManager] 레벨업 골드 부족. 필요: {goldCost:N0}");
                return false;
            }

            CurrencyManager.Instance.ChangeCurrency(CurrencyType.Gold, -goldCost);
            characterLevel++;

            baseAttackPower += nextLevelEntry.AttackIncrease;
            baseMaxHealth += nextLevelEntry.HealthIncrease;
        
            OnLevelUp?.Invoke(characterLevel);
        
            Debug.Log($"[CharacterManager] 레벨 업! Lv.{characterLevel}.");
            return true;
        }
    }
}