using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Bird.Idle.Data;
using Bird.Idle.Core;
using Bird.Idle.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Bird.Idle.Gameplay
{
    /// <summary>
    /// 모든 장비의 보유 수량, 컬렉션 레벨, 자동 판매, 영구 스탯 업그레이드를 관리하는 싱글톤 클래스.
    /// </summary>
    public class EquipmentCollectionManager : MonoBehaviour
    {
        public static EquipmentCollectionManager Instance { get; private set; }
        
        [Header("Data References")]
        [SerializeField] private AssetLabelReference allEquipmentLabel;
        
        [Header("Collection Settings")]
        [SerializeField] private int upgradeCostCount = 5;
        [SerializeField] private EquipmentGrade autoSellGradeThreshold = EquipmentGrade.Rare;
        
        [Header("UI References")]
        [SerializeField] private UpgradePopup upgradePopupPrefab;
        [SerializeField] private Transform upgradePopupTransform;
        
        private UpgradePopup activePopupInstance;
        
        private Task equipmentDataLoadTask;

        private Dictionary<int, CollectionEntry> collectionMap = new Dictionary<int, CollectionEntry>();
        private Dictionary<int, EquipmentData> allEquipmentSO = new Dictionary<int, EquipmentData>();
        
        public Action OnCollectionChanged;
        
        public Dictionary<int, EquipmentData> AllEquipmentSO { get; private set; } = new Dictionary<int, EquipmentData>();
        public int UpgradeCostCount => upgradeCostCount;
        public Task WaitForDataLoad() => equipmentDataLoadTask;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadAllEquipmentDataAsync();
        }
        
        /// <summary>
        /// GameManager에서 로드된 데이터를 받아 컬렉션 상태를 초기화
        /// </summary>
        public void Initialize(List<CollectionEntry> loadedEntries)
        {
            if (loadedEntries == null) return;
            
            foreach (var entry in loadedEntries)
            {
                if (collectionMap.ContainsKey(entry.equipID))
                {
                    collectionMap[entry.equipID].count = entry.count;
                    collectionMap[entry.equipID].collectionLevel = entry.collectionLevel;
                }
                else
                {
                    Debug.LogWarning($"[CollectionManager] 로드된 ID {entry.equipID}는 현재 정의되지 않은 아이템입니다. 무시합니다.");
                }
            }
            
            Debug.Log($"[CollectionManager] 컬렉션 데이터 로드 완료. 로드된 항목 수: {loadedEntries.Count}");
            OnCollectionChanged?.Invoke();
        }
        
        /// <summary>
        /// DataManager에 저장할 현재 컬렉션 데이터를 수집하여 반환
        /// </summary>
        public void CollectSaveData(GameSaveData data)
        {
            data.CollectionEntries = collectionMap.Values.ToList();
        }
        
        private async void LoadAllEquipmentDataAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            equipmentDataLoadTask = tcs.Task;
            
            AsyncOperationHandle<IList<EquipmentData>> handle = Addressables.LoadAssetsAsync<EquipmentData>(allEquipmentLabel, null);
            await handle.Task; 
        
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Dictionary<int, EquipmentData> loadedMap = new Dictionary<int, EquipmentData>();
                
                foreach (var soData in handle.Result)
                {
                    loadedMap.Add(soData.equipID, soData);
                
                    if (!collectionMap.ContainsKey(soData.equipID))
                    {
                        collectionMap.Add(soData.equipID, new CollectionEntry(soData.equipID));
                    }
                }
                
                AllEquipmentSO = loadedMap;
                Debug.Log($"[CollectionManager] 모든 장비 데이터 로드 및 컬렉션 맵 초기화 완료. (총 {AllEquipmentSO.Count}종)");
                tcs.SetResult(true);
            }
            else
            {
                Debug.LogError($"[CollectionManager] 장비 데이터 로드 실패: {handle.OperationException}");
                tcs.SetResult(false); // 로드 실패 신호
            }
        }

        /// <summary>
        /// 몬스터 처치 등으로 장비를 획득
        /// </summary>
        public void AddItem(EquipmentData item)
        {
            if (item == null) return;

            if (item.equipID <= 0)
            {
                Debug.LogError($"[Collection] 획득 장비의 ID가 유효하지 않습니다! ID: {item.equipID}. SO 파일을 확인하세요.");
                SellItem(item); 
                return;
            }
            
            if (item.grade < autoSellGradeThreshold)
            {
                SellItem(item);
            }
            else if (collectionMap.TryGetValue(item.equipID, out CollectionEntry entry))
            {
                entry.count++;
                Debug.Log($"[Collection] ID {item.equipID} 수량 증가 ({entry.count}개).");
                OnCollectionChanged?.Invoke(); 
            }
            else
            {
                Debug.LogWarning($"[Collection] ID {item.equipID}는 정의되지 않은 아이템이므로 판매합니다.");
                SellItem(item);
            }
        }
        
        private void SellItem(EquipmentData item)
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.ChangeCurrency(CurrencyType.Gold, item.sellPrice);
            }
            Debug.Log($"[Collection] {item.equipName} (Grade:{item.grade}) 자동 판매됨.");
        }
        
        public void ShowUpgradePopup(int equipID)
        {
            if (!collectionMap.TryGetValue(equipID, out CollectionEntry entry)) return;

            if (activePopupInstance == null)
            {
                activePopupInstance = Instantiate(upgradePopupPrefab, upgradePopupTransform, false);
            }

            activePopupInstance.Show(entry); 
        }
        
        /// <summary>
        /// 컬렉션 업그레이드를 시도하고 성공 여부를 반환
        /// </summary>
        /// <param name="equipID">업그레이드할 장비의 ID</param>
        /// <param name="goldCost">업그레이드에 필요한 골드 비용</param>
        /// <returns>업그레이드 성공 여부</returns>
        public bool TryUpgradeCollection(int equipID, long goldCost)
        {
            if (!collectionMap.TryGetValue(equipID, out CollectionEntry entry))
            {
                Debug.LogError($"[CollectionManager] ID {equipID} 컬렉션 항목이 맵에 없습니다.");
                return false;
            }
    
            if (!CanUpgrade(entry, goldCost))
            {
                Debug.LogWarning("[CollectionManager] 업그레이드 재료(수량 또는 골드)가 부족합니다.");
                return false;
            }

            entry.count -= upgradeCostCount; 
    
            if (!CurrencyManager.Instance.ChangeCurrency(CurrencyType.Gold, -goldCost))
            {
                Debug.LogError("[CollectionManager] 골드 소모 실패. 비상 상황!");
                return false; 
            }

            entry.collectionLevel++;
    
            if (AllEquipmentSO.TryGetValue(equipID, out EquipmentData item))
            {
                float upgradeAtk = item.attackBonus * 0.05f; 
                float upgradeHp = item.healthBonus * 0.05f;
                CharacterManager.Instance.ApplyBaseStatUpgrade(upgradeAtk, upgradeHp);
            }
    
            Debug.Log($"[Collection] ID {equipID} 컬렉션 업그레이드 성공! Lv.{entry.collectionLevel}");
    
            OnCollectionChanged?.Invoke(); 
    
            return true;
        }
        
        /// <summary>
        /// 업그레이드 가능 여부를 검사
        /// </summary>
        public bool CanUpgrade(CollectionEntry entry, long goldCost)
        {
            long currentGold = CurrencyManager.Instance.GetAmount(CurrencyType.Gold);
    
            return (entry.count >= upgradeCostCount) && (currentGold >= goldCost);
        }
        
        // UI가 모든 컬렉션 항목을 가져갈 수 있도록 메서드 제공
        public Dictionary<int, CollectionEntry> GetAllCollectionEntries() => collectionMap;
        
        // 특정 아이템의 수량 반환
        public int GetItemCount(int equipID) => collectionMap.ContainsKey(equipID) ? collectionMap[equipID].count : 0;
        
        // 특정 아이템의 레벨 반환
        public int GetCollectionLevel(int equipID) => collectionMap.ContainsKey(equipID) ? collectionMap[equipID].collectionLevel : 0;
    }
}