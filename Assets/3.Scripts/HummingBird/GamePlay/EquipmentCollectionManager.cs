using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Bird.Idle.Data;
using Bird.Idle.Core;
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

        private Dictionary<int, CollectionEntry> collectionMap = new Dictionary<int, CollectionEntry>();
        private Dictionary<int, EquipmentData> allEquipmentSO = new Dictionary<int, EquipmentData>();
        
        public Action OnCollectionChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadAllEquipmentDataAsync();
        }
        
        private async void LoadAllEquipmentDataAsync()
        {
            AsyncOperationHandle<IList<EquipmentData>> handle = Addressables.LoadAssetsAsync<EquipmentData>(allEquipmentLabel, null);

            await handle.Task; 
        
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var soData in handle.Result)
                {
                    allEquipmentSO.Add(soData.equipID, soData);
                
                    if (!collectionMap.ContainsKey(soData.equipID))
                    {
                        collectionMap.Add(soData.equipID, new CollectionEntry(soData.equipID));
                    }
                }
                Debug.Log($"[CollectionManager] 모든 장비 데이터 로드 및 컬렉션 맵 초기화 완료. (총 {allEquipmentSO.Count}종)");
            }
            else
            {
                Debug.LogError($"[CollectionManager] 장비 데이터 로드 실패: {handle.OperationException}");
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
            
            if (item.grade <= autoSellGradeThreshold)
            {
                SellItem(item);
            }
            else if (collectionMap.TryGetValue(item.equipID, out CollectionEntry entry))
            {
                entry.count++;
                Debug.Log($"[Collection] ID {item.equipID} 수량 증가 ({entry.count}개).");
                OnCollectionChanged?.Invoke(); 
                CheckForUpgrade(entry, item);
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

        private void CheckForUpgrade(CollectionEntry entry, EquipmentData item)
        {
            while (entry.count >= upgradeCostCount)
            {
                entry.count -= upgradeCostCount;
                entry.collectionLevel++;

                if (CharacterManager.Instance != null)
                {
                    float upgradeAtk = item.attackBonus * 0.05f;
                    float upgradeHp = item.healthBonus * 0.05f;
                    CharacterManager.Instance.ApplyBaseStatUpgrade(upgradeAtk, upgradeHp);
                }

                Debug.Log($"[Collection] {item.equipName} 컬렉션 레벨 업! Lv.{entry.collectionLevel}");
                OnCollectionChanged?.Invoke(); 
            }
        }
        
        // UI가 모든 컬렉션 항목을 가져갈 수 있도록 메서드 제공
        public Dictionary<int, CollectionEntry> GetAllCollectionEntries() => collectionMap;
        
        // 특정 아이템의 수량 반환
        public int GetItemCount(int equipID) => collectionMap.ContainsKey(equipID) ? collectionMap[equipID].count : 0;
        
        // 특정 아이템의 레벨 반환
        public int GetCollectionLevel(int equipID) => collectionMap.ContainsKey(equipID) ? collectionMap[equipID].collectionLevel : 0;
    }
}