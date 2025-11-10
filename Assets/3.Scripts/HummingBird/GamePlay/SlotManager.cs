using UnityEngine;
using System.Collections.Generic;
using System;
using Bird.Idle.Data;
using Bird.Idle.Core;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

namespace Bird.Idle.Gameplay
{
    /// <summary>
    /// 장비 칸(Slot) 강화 레벨 및 영구 스탯 증가를 관리하는 싱글톤 클래스
    /// </summary>
    public class SlotManager : MonoBehaviour
    {
        public static SlotManager Instance { get; private set; }

        // 장비 타입별 현재 강화 레벨 저장
        private Dictionary<EquipmentType, int> slotLevels = new Dictionary<EquipmentType, int>();
        
        [Header("Data References")]
        [SerializeField] private AssetReferenceT<SlotEnhanceData> slotEnhanceDataReference;

        private SlotEnhanceData loadedSlotEnhanceData;
        
        private Task slotDataLoadTask;
        
        public Action OnSlotEnhanceChanged;
        public Action OnSlotDataLoaded; // 데이터 로드가 완료되었음을 알리는 이벤트
        
        public Dictionary<EquipmentType, int> GetSlotLevels() => slotLevels;
        
        public Task WaitForDataLoad() => slotDataLoadTask;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
            {
                slotLevels[type] = 0; 
            }

            LoadSlotEnhanceDataAsync();
        }
        
        /// <summary>
        /// GameManager에서 로드된 데이터를 받아 슬롯 레벨을 초기화
        /// </summary>
        public void Initialize(Dictionary<EquipmentType, int> loadedSlotLevels)
        {
            if (loadedSlotLevels == null) return;
            
            // 레벨 복원
            foreach (var type in Enum.GetValues(typeof(EquipmentType)))
            {
                EquipmentType equipType = (EquipmentType)type;
                int levelToRestore = 0;

                if (loadedSlotLevels.TryGetValue(equipType, out int loadedLevel))
                {
                    levelToRestore = loadedLevel;
                }
                
                // ApplyPermanentBonus(equipType, levelToRestore);

                slotLevels[equipType] = levelToRestore;
            }

            Debug.Log($"[SlotManager] 슬롯 레벨 데이터 로드 완료. 무기 슬롯 레벨: {GetSlotLevel(EquipmentType.Weapon)}");
            OnSlotEnhanceChanged?.Invoke();
        }
        
        /// <summary>
        /// 로드된 레벨까지의 총 영구 스탯 보너스를 CharacterManager에 적용
        /// </summary>
        private void ApplyPermanentBonus(EquipmentType type, int targetLevel)
        {
            if (loadedSlotEnhanceData == null || CharacterManager.Instance == null) return;

            // 현재 레벨부터 목표 레벨까지 반복하여 스탯 합산
            float totalAttackIncrease = 0f;
            float totalHealthIncrease = 0f;

            for (int i = 1; i <= targetLevel; i++)
            {
                SlotEnhanceData.SlotEnhanceEntry entry = loadedSlotEnhanceData.GetEnhanceEntry(type, i - 1);
                if (entry.EnhanceLevel != -1)
                {
                    totalAttackIncrease += entry.AttackIncrease;
                    totalHealthIncrease += entry.HealthIncrease;
                }
            }

            // CharacterManager에 합산된 보너스를 한 번에 적용
            if (totalAttackIncrease > 0 || totalHealthIncrease > 0)
            {
                CharacterManager.Instance.ApplyBaseStatUpgrade(totalAttackIncrease, totalHealthIncrease);
            }
        }
        
        /// <summary>
        /// DataManager에 저장할 현재 슬롯 레벨을 수집하여 GameSaveData에 추가
        /// </summary>
        public void CollectSaveData(GameSaveData data)
        {
            data.SlotLevels = slotLevels;
        }
        
        private async void LoadSlotEnhanceDataAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            slotDataLoadTask = tcs.Task;
            
            AsyncOperationHandle<SlotEnhanceData> handle = slotEnhanceDataReference.LoadAssetAsync<SlotEnhanceData>();
            await handle.Task;
        
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedSlotEnhanceData = handle.Result;
                Debug.Log("[SlotManager] SlotEnhanceData Addressables 로드 완료!");
                
                // OnSlotDataLoaded?.Invoke();
                tcs.SetResult(true);
            }
            else
            {
                Debug.LogError($"[SlotManager] SlotEnhanceData 로드 실패: {handle.OperationException}");
                tcs.SetResult(false);
            }
        }
        
        /// <summary>
        /// 장비 칸 강화 시도
        /// </summary>
        public bool TryEnhanceSlot(EquipmentType type)
        {
            if (loadedSlotEnhanceData == null) return false;
            
            int currentLevel = slotLevels[type];
            SlotEnhanceData.SlotEnhanceEntry nextEntry = loadedSlotEnhanceData.GetEnhanceEntry(type, currentLevel);

            if (nextEntry.EnhanceLevel == -1)
            {
                Debug.Log($"[SlotManager] {type} 슬롯이 최대 강화 레벨입니다.");
                return false;
            }

            if (!CurrencyManager.Instance.CanAfford(CurrencyType.Gold, nextEntry.GoldCost))
            {
                Debug.LogWarning("[SlotManager] 강화 골드 부족.");
                return false;
            }
            CurrencyManager.Instance.ChangeCurrency(CurrencyType.Gold, -nextEntry.GoldCost);

            if (UnityEngine.Random.value <= nextEntry.SuccessRate)
            {
                slotLevels[type]++;
                CharacterManager.Instance.ApplyBaseStatUpgrade(nextEntry.AttackIncrease, nextEntry.HealthIncrease);
                
                OnSlotEnhanceChanged?.Invoke();
                Debug.Log($"[SlotManager] {type} 슬롯 강화 성공! Lv.{slotLevels[type]}");
                return true;
            }
            else
            {
                Debug.LogWarning("[SlotManager] 슬롯 강화 실패. 골드만 소모됨.");
                return false;
            }
        }
        
        public int GetSlotLevel(EquipmentType type) => slotLevels[type];
        public SlotEnhanceData GetSlotEnhanceData() => loadedSlotEnhanceData;
    }
}