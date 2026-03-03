using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Bird.Idle.Core;
using Bird.Idle.Data;
using Bird.Idle.UI;
using Bird.Idle.Utils;
using Bird.Idle.Visual;
using Unity.VisualScripting;

namespace Bird.Idle.Gameplay
{
    /// <summary>
    /// 몬스터 스폰 및 처치를 관리하고, 처치 시 재화를 지급 (오브젝트 풀링 추가)
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }
        
        [Header("Data References")]
        [SerializeField] private AssetLabelReference monsterDataLabel; // 라벨 기반 컬렉션 로드용 AssetLabelRefrence라 함.
        
        [Header("Spawn Settings")]
        [SerializeField] private float spawnInterval = 4f; // 몬스터 스폰 주기
        [SerializeField] private int maxMonsterCount = 15; // 최대 몬스터 수
        [SerializeField] private Vector3 spawnPosition = new Vector3(4.5f, 0f, 0f);
        [SerializeField] private Quaternion spawnRotation = new Quaternion(0f, 180f, 0f, 0f);
        
        [Header("Other Settings")]
        [SerializeField] private PlayerController playerController; // Player의 Attack 애니메이션 동안은, BattleMode 전환을 잠가놓기 위한 참조.

        private float currentSpawnTime;
        private int currentMonsterCount = 0;
        private int totalSpawnedInCurrentStage = 0;
        
        private bool isInfiniteSpawnMode = false; // 몬스터 스폰 무한 모드
        
        private Dictionary<int, MonsterData> loadedMonsterDictionary = new Dictionary<int, MonsterData>();
        private Dictionary<int, Queue<MonsterController>> monsterPools = new Dictionary<int, Queue<MonsterController>>();
        
        private StageData currentStageData;
        private List<int> currentStageMonsterIDs;
        
        private List<MonsterController> activeMonsters = new List<MonsterController>();
        private int monsterInstanceCounter = 0;
        private MonsterController frontMonster;
        
        private bool isDataLoaded = false;
        
        private float currentGameSpeed = 1.0f;
        
        public bool IsDataLoaded => isDataLoaded;

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
        
        private void Start()
        {
            if (BattleManager.Instance != null)
            {
                currentGameSpeed = BattleManager.Instance.GameSpeed;
                BattleManager.Instance.OnGameSpeedChanged += HandleGameSpeedChanged;
            }
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
                isDataLoaded = true;
            }
            else
            {
                Debug.LogError($"[EnemyManager] MonsterData 로드 실패");
            }
        }

        private void Update()
        {
            if (!isDataLoaded) return;
            
            currentSpawnTime += Time.deltaTime * currentGameSpeed;

            bool canSpawnMore = currentStageData != null && (isInfiniteSpawnMode || totalSpawnedInCurrentStage < currentStageData.MonsterKillCountRequired);
            
            if (canSpawnMore && currentMonsterCount < maxMonsterCount && currentSpawnTime >= spawnInterval)
            {
                SpawnMonster();
                currentSpawnTime = 0f;
            }
            
            CheckBattleState();
        }
        
        private void OnDestroy()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnGameSpeedChanged -= HandleGameSpeedChanged;
            }
        }
        
        private void HandleGameSpeedChanged(float newSpeed)
        {
            currentGameSpeed = newSpeed;
        }
        
        /// <summary>
        /// 최전방 몬스터의 위치를 기반으로 전투 상태를 확인하고 BattleManager에 전달
        /// </summary>
        private void CheckBattleState()
        {
            if(playerController.GetAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == playerController.GetAttackAnimHash)
            {
                return;
            }
            
            if (frontMonster == null)
            {
                BattleManager.Instance.SetBattleActive(false);
                return;
            }
            
            Vector3 targetPosition = PlayerController.PlayerTransform.position;

            float distanceToPlayer = Vector3.Distance(frontMonster.transform.position, targetPosition);
            
            if (distanceToPlayer <= CharacterManager.Instance.PlayerAttackRange)
            {
                BattleManager.Instance.SetBattleActive(true);
            }
            else
            {
                BattleManager.Instance.SetBattleActive(false);
            }
        }
        
        /// <summary>
        /// StageManager로부터 현재 스테이지 정보를 업데이트
        /// </summary>
        public void UpdateStageData(StageData data, int currentProgress = 0, bool isFarmingMode = false)
        {
            currentStageData = data;
            currentStageMonsterIDs = data.MonsterIDs;
            
            // 파밍 모드면 무한 모드 활성화
            isInfiniteSpawnMode = isFarmingMode;
            
            totalSpawnedInCurrentStage = currentProgress;
        }

        private void SpawnMonster()
        {
            if (loadedMonsterDictionary.Count == 0 || currentStageData == null)
            {
                Debug.LogWarning("[EnemyManager] 데이터가 없습니다.");
                return;
            }

            if (playerController.GetAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == playerController.GetDeathAnimHash)
            {
                return; // Player가 죽은 상태면 스폰을 멈춰요.
            }
            
            int monsterIdToSpawn = -1;
            
            if (currentStageData.IsBossStage)
            {
                monsterIdToSpawn = currentStageData.BossMonsterID;
                if (currentMonsterCount > 0) return; 
            }
            else
            {
                if (currentStageMonsterIDs == null || currentStageMonsterIDs.Count == 0) return;
                int randomIndex = UnityEngine.Random.Range(0, currentStageMonsterIDs.Count);
                monsterIdToSpawn = currentStageMonsterIDs[randomIndex];
            }
            
            if (loadedMonsterDictionary.TryGetValue(monsterIdToSpawn, out MonsterData monsterData))
            {
                SpawnMonsterFromAddress(monsterData);
            }
            else
            {
                Debug.LogError($"[EnemyManager] ID {monsterIdToSpawn} 몬스터 데이터가 없습니다!");
            }
        }
        
        /// <summary>
        /// Addressables를 사용하여 몬스터 프리팹을 로드
        /// </summary>
        private async void SpawnMonsterFromAddress(MonsterData monsterData)
        {
            int id = monsterData.monsterID;
            
            if (monsterPools.TryGetValue(id, out Queue<MonsterController> pool) && pool.Count > 0)
            {
                MonsterController pooledMonster = pool.Dequeue();
                pooledMonster.gameObject.SetActive(true);
                SetupMonster(pooledMonster, monsterData);
                return;
            }
            
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(monsterData.prefabAddress);
            await handle.Task;
        
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[EnemyManager] {monsterData.monsterName} 프리팹 로드 실패");
                return;
            }

            GameObject monsterGO = handle.Result;
            MonsterController newController = monsterGO.GetComponent<MonsterController>();

            if (newController == null)
            {
                Addressables.ReleaseInstance(handle); 
                return;
            }
            
            SetupMonster(newController, monsterData);
        }
        
        /// <summary>
        /// 몬스터 스폰 시 공통 세팅 로직
        /// </summary>
        private void SetupMonster(MonsterController controller, MonsterData monsterData)
        {
            float prefabOriginalY = controller.transform.position.y;
            float prefabOriginalZ = controller.transform.position.z;
            
            controller.transform.position = new Vector3(spawnPosition.x, prefabOriginalY, prefabOriginalZ);
            controller.transform.rotation = spawnRotation;

            monsterInstanceCounter++;
            controller.Initialize(monsterData, 1.0f, monsterInstanceCounter);
        
            totalSpawnedInCurrentStage++;
            
            activeMonsters.Add(controller);
            currentMonsterCount = activeMonsters.Count;
        
            if (frontMonster == null)
            {
                frontMonster = controller;
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

            float goldBonus = 1.0f;
            if (CharacterManager.Instance != null && CharacterManager.Instance.PlayerStats != null)
            {
                goldBonus = CharacterManager.Instance.PlayerStats.FinalGoldDrop;
            }
            
            // 최종 골드 연산 (기본보상 * 스테이지배율 * LUK 스탯 배율)
            long baseGold = (long)(monsterData.goldReward * currentStageData.GoldRewardMultiplier);
            long finalGoldReward = (long)(baseGold * goldBonus);
            long finalExpReward = (long)(monsterData.expReward * currentStageData.ExpRewardMultiplier);
            
            if (CharacterManager.Instance != null && finalExpReward > 0)
            {
                CharacterManager.Instance.GainExperience(finalExpReward);
                if (RewardLogManager.Instance != null)
                    RewardLogManager.Instance.ShowLog(RewardLogManager.Instance.expIcon, $"+ {BigNumberFormatter.Format(finalExpReward)} EXP", Color.cyan);
            }
            
            if (finalGoldReward > 0)
            {
                CurrencyManager.Instance.ChangeCurrency(CurrencyType.Gold, finalGoldReward);
                if (RewardLogManager.Instance != null)
                    RewardLogManager.Instance.ShowLog(RewardLogManager.Instance.goldIcon, $"+ {BigNumberFormatter.Format(finalGoldReward)} Gold", Color.yellow);
            }
            
            DropEquipment(monsterData.dropTable);
            RemoveDefeatedMonster(monsterData.monsterID);
        }
        
        private void RemoveDefeatedMonster(int monsterID)
        {
            MonsterController defeated = activeMonsters.Find(m => m.MonsterData.monsterID == monsterID);
            if (defeated != null)
            {
                activeMonsters.Remove(defeated);
                currentMonsterCount = activeMonsters.Count;
                
                frontMonster = activeMonsters.Count > 0 ? activeMonsters[0] : null;
                
                CheckBattleState();
            }
        }
        
        /// <summary>
        /// 몬스터를 비활성화하고 큐에 반환
        /// </summary>
        public void ReturnMonsterToPool(MonsterController controller)
        {
            if (controller == null) return;

            controller.gameObject.SetActive(false);
            
            int id = controller.MonsterData.monsterID;
            if (!monsterPools.ContainsKey(id))
            {
                monsterPools[id] = new Queue<MonsterController>();
            }
            monsterPools[id].Enqueue(controller);
        }
        
        /// <summary>
        /// 필드에 있는 모든 몬스터를 삭제
        /// </summary>
        public void ClearAllMonsters()
        {
            if (activeMonsters == null) return;

            for (int i = activeMonsters.Count - 1; i >= 0; i--)
            {
                var monster = activeMonsters[i];
                if (monster != null)
                {
                    monster.StopAllCoroutines();
                    ReturnMonsterToPool(monster);
                }
            }
            
            activeMonsters.Clear();
            currentMonsterCount = 0;
            frontMonster = null;
            totalSpawnedInCurrentStage = 0;
            
            CheckBattleState();
        }
        
        /// <summary>
        /// 드롭 테이블을 사용하여 장비 드롭 시도
        /// </summary>
        private void DropEquipment(List<DropItem> dropTable)
        {
            if (dropTable == null || dropTable.Count == 0) return;

            float itemDropBonus = 1.0f;
            if (CharacterManager.Instance != null && CharacterManager.Instance.PlayerStats != null)
            {
                itemDropBonus = CharacterManager.Instance.PlayerStats.FinalItemDrop;
            }
            
            // 전체 확률 총합 계산 (기본 확률 * 스탯 보너스)
            float totalChance = 0f;
            foreach (var dropItem in dropTable)
            {
                totalChance += dropItem.dropRate;
            }

            float randomValue = UnityEngine.Random.value * totalChance;
            float cumulative = 0f;

            foreach (var dropItem in dropTable)
            {
                float chance = dropItem.dropRate * itemDropBonus;
                cumulative += chance;
                
                if (randomValue <= cumulative)
                {
                    EquipmentCollectionManager.Instance.AddItem(dropItem.itemSO);

                    if (RewardLogManager.Instance != null)
                    {
                        Color nameColor = GetColorByGrade(dropItem.itemSO.grade);
                        RewardLogManager.Instance.ShowLog(dropItem.itemSO.iconAddress, $"+ EquipItem", nameColor);
                    }
                    return; 
                }
            }
        }
        
        // 장비 등급별 색상 반환 함수
        private Color GetColorByGrade(EquipmentGrade grade)
        {
            switch (grade)
            {
                case EquipmentGrade.Common: return Color.white;
                case EquipmentGrade.Rare: return new Color(0.2f, 0.8f, 1f); // 파랑
                case EquipmentGrade.Epic: return new Color(0.8f, 0.2f, 1f); // 보라
                case EquipmentGrade.Legendary: return new Color(1f, 0.8f, 0.2f); // 주황
                default: return Color.white;
            }
        }
        
        /// <summary>
        /// 몬스터 ID로 MonsterData를 반환 (방치 보상 계산용)
        /// </summary>
        public MonsterData GetMonsterData(int monsterID)
        {
            if (loadedMonsterDictionary.TryGetValue(monsterID, out MonsterData data)) return data;
            return null;
        }
    }
}