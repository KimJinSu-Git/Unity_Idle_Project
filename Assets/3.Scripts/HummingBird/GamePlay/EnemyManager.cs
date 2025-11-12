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
        
        [SerializeField] private float spawnInterval = 1.0f; // 몬스터 스폰 주기
        [SerializeField] private int maxMonsterCount = 5; // 최대 몬스터 수

        private float currentSpawnTime;
        private int currentMonsterCount = 0;
        
        private Dictionary<int, MonsterData> loadedMonsterDictionary = new Dictionary<int, MonsterData>();
        
        private StageData currentStageData;
        private List<int> currentStageMonsterIDs;
        
        private List<MonsterController> activeMonsters = new List<MonsterController>();
        private int monsterInstanceCounter = 0;
        private MonsterController frontMonster;

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
            
            CheckBattleState();
        }
        
        /// <summary>
        /// 최전방 몬스터의 위치를 기반으로 전투 상태를 확인하고 BattleManager에 전달
        /// </summary>
        private void CheckBattleState()
        {
            if (frontMonster == null)
            {
                BattleManager.Instance.SetBattleActive(false);
                return;
            }

            // TODO: Player의 위치를 기반으로 거리를 측정하는 로직 필요
            float distanceToPlayer = Vector3.Distance(frontMonster.transform.position, Vector3.zero);
            
            if (distanceToPlayer <= frontMonster.AttackRange)
            {
                BattleManager.Instance.SetBattleActive(true);
            }
            else
            {
                BattleManager.Instance.SetBattleActive(false); // 이동
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
            
            int randomIndex = UnityEngine.Random.Range(0, currentStageMonsterIDs.Count);
            int monsterIdToSpawn = currentStageMonsterIDs[randomIndex];
            
            if (loadedMonsterDictionary.TryGetValue(monsterIdToSpawn, out MonsterData monsterData))
            {
                // 임시: 몬스터 오브젝트를 생성하고 Controller를 할당
                GameObject monsterGO = new GameObject("Monster Placeholder"); 
                MonsterController controller = monsterGO.AddComponent<MonsterController>();
                
                monsterInstanceCounter++;
                
                controller.Initialize(monsterData, 1.0f, monsterInstanceCounter);
                
                activeMonsters.Add(controller);
                currentMonsterCount = activeMonsters.Count;
                
                if (frontMonster == null)
                {
                    frontMonster = controller;
                }
            }
            else
            {
                Debug.LogError($"[EnemyManager] ID {monsterIdToSpawn} 몬스터 데이터가 없습니다!");
            }
        }
        
        public void ApplyDamageToCurrentMonster(float damage)
        {
            if (frontMonster == null || !frontMonster.IsAlive) return;
            
            frontMonster.ApplyDamage(damage);
        }
        
        /// <summary>
        /// 몬스터 처치 시 호출되어 보상을 지급하고 StageManager에 알림
        /// </summary>
        public void ProcessMonsterDefeat(MonsterData monsterData)
        {
            if (monsterData == null || currentStageData == null) return;

            StageManager.Instance.OnMonsterKilled();
            
            // 보상 지급
            long goldReward = (long)(monsterData.goldReward * currentStageData.GoldRewardMultiplier);
            long expReward = (long)(monsterData.expReward * currentStageData.ExpRewardMultiplier);
            
            CurrencyManager.Instance.ChangeCurrency(CurrencyType.Gold, goldReward);
            
            DropEquipment(monsterData.dropTable);
            
            RemoveDefeatedMonster(monsterData.monsterID); 
        
            Debug.Log($"[EnemyManager] 몬스터 처치 완료.");
        }
        
        private void RemoveDefeatedMonster(int monsterID)
        {
            MonsterController defeated = activeMonsters.Find(m => m.MonsterData.monsterID == monsterID);
            if (defeated != null)
            {
                activeMonsters.Remove(defeated);
                currentMonsterCount = activeMonsters.Count;
                
                frontMonster = activeMonsters.Count > 0 ? activeMonsters[0] : null;
            }
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

        // [ContextMenu("몬스터 즉시 처치")]
        // public void TestKillMonster() => KillCurrentMonster();
    }
}