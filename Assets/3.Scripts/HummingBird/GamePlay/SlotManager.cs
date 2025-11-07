using UnityEngine;
using System.Collections.Generic;
using System;
using Bird.Idle.Data;
using Bird.Idle.Core;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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
        
        public Action OnSlotEnhanceChanged;
        public Action OnSlotDataLoaded; // 데이터 로드가 완료되었음을 알리는 이벤트

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
        
        private async void LoadSlotEnhanceDataAsync()
        {
            AsyncOperationHandle<SlotEnhanceData> handle = slotEnhanceDataReference.LoadAssetAsync<SlotEnhanceData>();
            await handle.Task;
        
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedSlotEnhanceData = handle.Result;
                Debug.Log("[SlotManager] SlotEnhanceData Addressables 로드 완료!");
                
                OnSlotDataLoaded?.Invoke();
            }
            else
            {
                Debug.LogError($"[SlotManager] SlotEnhanceData 로드 실패: {handle.OperationException}");
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