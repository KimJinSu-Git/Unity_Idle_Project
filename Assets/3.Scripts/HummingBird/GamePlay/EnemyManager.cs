using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Bird.Idle.Core;
using Bird.Idle.Data;

namespace Bird.Idle.Gameplay
{
    /// <summary>
    /// 몬스터 스폰 및 처치를 관리하고, 처치 시 재화를 지급
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }
        
        [Header("Data References")]
        [SerializeField] private AssetLabelReference monsterDataLabel; // 라벨 기반 컬렉션 로드용 AssetLabelRefrence라 함.
        
        [Header("Loot Drop Settings")]
        [SerializeField] private float dropChance = 0.1f;
        [SerializeField] private List<EquipmentData> droppableEquipment;
        
        [SerializeField] private float spawnInterval = 1.0f; // 몬스터 스폰 주기
        [SerializeField] private int maxMonsterCount = 5; // 최대 몬스터 수

        private float currentSpawnTime;
        private int currentMonsterCount = 0;
        
        private Dictionary<int, MonsterData> loadedMonsterDictionary = new Dictionary<int, MonsterData>();
        
        private StageData currentStageData;
        private List<int> currentStageMonsterIDs;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadMonsterDataAsync();
        }
        
        /// <summary>
        /// Addressables를 사용하여 'Enemy' 라벨의 모든 MonsterData를 로드
        /// </summary>
        private async void LoadMonsterDataAsync()
        {
            AsyncOperationHandle<IList<MonsterData>> handle = Addressables.LoadAssetsAsync<MonsterData>(monsterDataLabel, null);

            await handle.Task; 
        
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var monsterData in handle.Result)
                {
                    loadedMonsterDictionary.Add(monsterData.monsterID, monsterData);
                }
                Debug.Log($"[EnemyManager] MonsterData Addressables 로드 완료! (총 {loadedMonsterDictionary.Count}종)");
            }
            else
            {
                Debug.LogError($"[EnemyManager] MonsterData 로드 실패: {handle.OperationException}");
            }
        }

        private void Update()
        {
            currentSpawnTime += Time.deltaTime;

            if (currentMonsterCount < maxMonsterCount && currentSpawnTime >= spawnInterval)
            {
                SpawnMonster();
                currentSpawnTime = 0f;
            }
        }
        
        /// <summary>
        /// StageManager로부터 현재 스테이지 정보를 업데이트
        /// </summary>
        public void UpdateStageData(StageData data)
        {
            currentStageData = data;
            currentStageMonsterIDs = data.MonsterIDs;
        }

        private void SpawnMonster()
        {
            if (loadedMonsterDictionary.Count == 0 || currentStageMonsterIDs == null || currentStageMonsterIDs.Count == 0)
            {
                Debug.LogWarning("[EnemyManager] 몬스터 데이터 또는 스테이지 목록이 없습니다.");
                return;
            }
            currentMonsterCount++;
            
            int randomIndex = UnityEngine.Random.Range(0, currentStageMonsterIDs.Count);
            int monsterIdToSpawn = currentStageMonsterIDs[randomIndex];
            
            if (loadedMonsterDictionary.TryGetValue(monsterIdToSpawn, out MonsterData monsterData))
            {
                // TODO: monsterData.prefabAddress를 사용해 Addressables 비동기 로드 후 인스턴스화
        
                Debug.Log($"[EnemyManager] {monsterData.monsterName} (ID: {monsterIdToSpawn}) 스폰! 현재 수: {currentMonsterCount}");
            }
            else
            {
                Debug.LogError($"[EnemyManager] ID {monsterIdToSpawn} 몬스터 데이터가 없습니다!");
            }
        }

        /// <summary>
        /// 몬스터 처치 시 호출되어 보상을 지급하고 StageManager에 알림.
        /// </summary>
        public void KillMonster()
        {
            if (currentMonsterCount <= 0 || loadedMonsterDictionary.Count == 0) return;
            
            StageManager.Instance.OnMonsterKilled();
            
            int rewardMonsterId = currentStageMonsterIDs[0];
            
            if (!loadedMonsterDictionary.TryGetValue(rewardMonsterId, out MonsterData baseMonster))
            {
                Debug.LogError($"[EnemyManager] ID {rewardMonsterId} 몬스터 보상 데이터 로드 실패. (Dict에 없음)");
                return;
            }

            long goldReward = (long)(baseMonster.goldReward * currentStageData.GoldRewardMultiplier);
            long expReward = (long)(baseMonster.expReward * currentStageData.ExpRewardMultiplier);
            
            currentMonsterCount--;

            CurrencyManager.Instance.ChangeCurrency(CurrencyType.Gold, goldReward);
            CharacterManager.Instance.GainExperience(expReward);
            
            if (UnityEngine.Random.value < dropChance)
            {
                DropEquipment();
            }
            
            Debug.Log($"[EnemyManager] 몬스터 처치! 골드 {goldReward} (x{currentStageData.GoldRewardMultiplier}), EXP {expReward} 획득.");
        }
        
        private void DropEquipment()
        {
            if (droppableEquipment == null || droppableEquipment.Count == 0) return;
    
            // 드롭할 장비 무작위 선택
            int randomIndex = UnityEngine.Random.Range(0, droppableEquipment.Count);
            EquipmentData droppedItem = droppableEquipment[randomIndex];

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(droppedItem);
            }
        }

        [ContextMenu("몬스터 즉시 처치")]
        public void TestKillMonster() => KillMonster();
    }
}