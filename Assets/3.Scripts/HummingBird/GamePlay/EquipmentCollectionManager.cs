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
        
        [Header("UI References")]
        [SerializeField] private UpgradePopup upgradePopupPrefab;
        [SerializeField] private Transform upgradePopupTransform;
        
        private UpgradePopup activePopupInstance;
        
        private Task equipmentDataLoadTask;

        private Dictionary<int, CollectionEntry> collectionMap = new Dictionary<int, CollectionEntry>();
        private Dictionary<int, EquipmentData> allEquipmentSO = new Dictionary<int, EquipmentData>();
        private Dictionary<EquipmentGrade, int> masukRewardTable = new Dictionary<EquipmentGrade, int>()
        {
            { EquipmentGrade.Common, 10 },
            { EquipmentGrade.Rare, 150 },
            { EquipmentGrade.Epic, 500 },
            { EquipmentGrade.Legendary, 2000 }
        };
        
        public Action OnCollectionChanged;
        
        public Dictionary<int, EquipmentData> AllEquipmentSO { get; private set; } = new Dictionary<int, EquipmentData>();
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
            if (item.equipID <= 0) return;
            
            if (collectionMap.TryGetValue(item.equipID, out CollectionEntry entry))
            {
                if (entry.count > 0)
                {
                    int rewardAmount = GetMasukAmount(item.grade);

                    if (CurrencyManager.Instance != null)
                    {
                        CurrencyManager.Instance.ChangeCurrency(CurrencyType.Masuk, rewardAmount);
                    }

                    Debug.Log($"[Collection] 중복 장비 {item.equipName} 획득 (현재 {entry.count}개 보유 중) -> 마석 {rewardAmount}개 변환");
                }
                else
                {
                    entry.count = 1;

                    Debug.Log($"[Collection] 신규 장비 {item.equipName} 최초 획득! (Count: 0 -> 1)");
                    OnCollectionChanged?.Invoke();
                }
            }
            else
            {
                CollectionEntry newEntry = new CollectionEntry(item.equipID);
                newEntry.count = 1;
                newEntry.collectionLevel = 1;

                collectionMap.Add(item.equipID, newEntry);

                Debug.Log($"[Collection] 미등록 장비 {item.equipName} 신규 등록 완료!");
                OnCollectionChanged?.Invoke();
            }
        }
        
        private int GetMasukAmount(EquipmentGrade grade)
        {
            if (masukRewardTable.TryGetValue(grade, out int amount))
                return amount;
            return 10;
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
        public bool TryUpgradeCollection(int equipID)
        {
            if (!collectionMap.TryGetValue(equipID, out CollectionEntry entry)) return false;
    
            long goldCost = CalculateGoldCost(entry.collectionLevel);
            long masukCost = CalculateMasukCost(entry.collectionLevel);

            if (!CanUpgrade(goldCost, masukCost))
            {
                Debug.LogWarning("업그레이드 재화 부족 (골드 or 마석)");
                return false;
            }

            CurrencyManager.Instance.ChangeCurrency(CurrencyType.Gold, -goldCost);
            CurrencyManager.Instance.ChangeCurrency(CurrencyType.Masuk, -masukCost);

            entry.collectionLevel++;
    
            if (AllEquipmentSO.TryGetValue(equipID, out EquipmentData item))
            {
                float upgradeAtk = item.attackBonus * 0.05f; 
                float upgradeHp = item.healthBonus * 0.05f;
                CharacterManager.Instance.ApplyBaseStatUpgrade(upgradeAtk, upgradeHp);
            }
    
            OnCollectionChanged?.Invoke(); 
    
            return true;
        }
        
        public long CalculateGoldCost(int currentLevel) => 1000 * (currentLevel + 1); 
        public long CalculateMasukCost(int currentLevel) => 50 * (currentLevel + 1);
        
        /// <summary>
        /// 업그레이드 가능 여부를 검사
        /// </summary>
        private bool CanUpgrade(long goldNeeded, long masukNeeded)
        {
            long currentGold = CurrencyManager.Instance.GetAmount(CurrencyType.Gold);
            long currentMasuk = CurrencyManager.Instance.GetAmount(CurrencyType.Masuk);
            return currentGold >= goldNeeded && currentMasuk >= masukNeeded;
        }
        
        // UI가 모든 컬렉션 항목을 가져갈 수 있도록 메서드 제공
        public Dictionary<int, CollectionEntry> GetAllCollectionEntries() => collectionMap;
        
        // 특정 아이템의 수량 반환
        public int GetItemCount(int equipID) => collectionMap.ContainsKey(equipID) ? collectionMap[equipID].count : 0;
        
        // 특정 아이템의 레벨 반환
        public int GetCollectionLevel(int equipID) => collectionMap.ContainsKey(equipID) ? collectionMap[equipID].collectionLevel : 0;
    }
}