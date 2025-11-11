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
        
        // [Header("Loot Drop Settings")]
        // [SerializeField] private float dropChance = 0.1f;
        // [SerializeField] private List<EquipmentData> droppableEquipment;
        
        [SerializeField] private float spawnInterval = 1.0f; // 몬스터 스폰 주기
        [SerializeField] private int maxMonsterCount = 5; // 최대 몬스터 수

        private float currentSpawnTime;
        private int currentMonsterCount = 0;
        
        private Dictionary<int, MonsterData> loadedMonsterDictionary = new Dictionary<int, MonsterData>();
        
        private StageData currentStageData;
        private List<int> currentStageMonsterIDs;
        
        private float currentMonsterHealth; 
        private MonsterData currentSpawnedMonsterData;

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
                currentMonsterCount++;
                currentSpawnedMonsterData = monsterData;
                
                currentMonsterHealth = monsterData.baseHealth;
        
                Debug.Log($"[EnemyManager] {monsterData.monsterName} (ID: {monsterIdToSpawn}) 스폰! 현재 수: {currentMonsterCount}");
            }
            else
            {
                Debug.LogError($"[EnemyManager] ID {monsterIdToSpawn} 몬스터 데이터가 없습니다!");
            }
        }
        
        public void ApplyDamageToCurrentMonster(float damage)
        {
            if (currentSpawnedMonsterData == null || currentMonsterHealth <= 0) return;
            
            currentMonsterHealth -= damage;
            
            Debug.Log($"[EnemyManager] 몬스터 ({currentSpawnedMonsterData?.monsterName}) 피격! 남은 HP: {currentMonsterHealth:F0}");
            
            if (currentMonsterHealth <= 0)
            {
                KillCurrentMonster();
            }
        }
        
        /// <summary>
        /// 몬스터 처치 시 호출되어 보상을 지급하고 StageManager에 알림.
        /// </summary>
        public void KillCurrentMonster()
        {
            if (currentMonsterCount <= 0 || currentSpawnedMonsterData == null || currentStageData == null) return;
            
            StageManager.Instance.OnMonsterKilled();
            
            // 보상 계산은 현재 스폰된 몬스터 데이터를 사용
            long goldReward = (long)(currentSpawnedMonsterData.goldReward * currentStageData.GoldRewardMultiplier);
            long expReward = (long)(currentSpawnedMonsterData.expReward * currentStageData.ExpRewardMultiplier);
            
            currentMonsterCount--;

            CurrencyManager.Instance.ChangeCurrency(CurrencyType.Gold, goldReward);
            
            // 몬스터별 드롭 테이블 사용
            DropEquipment(currentSpawnedMonsterData.dropTable);
            
            Debug.Log($"[EnemyManager] 몬스터 처치! 골드 {goldReward}, EXP {expReward} 획득.");
            
            currentSpawnedMonsterData = null;
        }
        
        /// <summary>
        /// 드롭 테이블을 사용하여 장비 드롭 시도
        /// </summary>
        private void DropEquipment(List<DropItem> dropTable)
        {
            if (dropTable == null || dropTable.Count == 0) return;

            float totalChance = 0f;
            foreach (var dropItem in dropTable)
            {
                totalChance += dropItem.dropRate;
            }

            float randomValue = UnityEngine.Random.value * totalChance;
            float cumulative = 0f;

            foreach (var dropItem in dropTable)
            {
                cumulative += dropItem.dropRate;
                if (randomValue <= cumulative)
                {
                    EquipmentCollectionManager.Instance.AddItem(dropItem.itemSO);
                    Debug.Log($"[EnemyManager] 장비 드롭 성공: {dropItem.itemSO.equipName}");
                    return; 
                }
            }
        }

        [ContextMenu("몬스터 즉시 처치")]
        public void TestKillMonster() => KillCurrentMonster();
    }
}