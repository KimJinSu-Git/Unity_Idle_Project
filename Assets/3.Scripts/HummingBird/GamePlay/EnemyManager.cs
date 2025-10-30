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
        // [SerializeField] private List<MonsterData> stageMonsterList;
        
        [SerializeField] private float spawnInterval = 1.0f; // 몬스터 스폰 주기
        // [SerializeField] private long goldPerKill = 100; // 몬스터 처치당 골드
        // [SerializeField] private long expPerKill = 50;  // 몬스터 처치당 경험치
        [SerializeField] private int maxMonsterCount = 5; // 최대 몬스터 수

        private float currentSpawnTime;
        private int currentMonsterCount = 0;
        
        private Dictionary<int, MonsterData> loadedMonsterDictionary = new Dictionary<int, MonsterData>();

        // public long GoldPerKill => goldPerKill;

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

        private void SpawnMonster()
        {
            currentMonsterCount++;
            // TODO: 몬스터 프리팹 인스턴스화 로직
            Debug.Log($"[EnemyManager] 몬스터 스폰! 현재 수: {currentMonsterCount}");
        }

        /// <summary>
        /// 몬스터 처치 시 호출되어 보상을 지급
        /// </summary>
        public void KillMonster()
        {
            if (currentMonsterCount <= 0 || loadedMonsterDictionary.Count == 0) return;
            
            if (!loadedMonsterDictionary.TryGetValue(1, out MonsterData currentMonster))
            {
                Debug.LogError("[EnemyManager] ID 1인 몬스터 데이터가 없습니다.");
                return;
            }

            currentMonsterCount--;

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.ChangeCurrency(CurrencyType.Gold, currentMonster.goldReward);
            }
            
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.GainExperience(currentMonster.expReward);
            }
            
            Debug.Log($"[EnemyManager] 몬스터 처치! 골드 {currentMonster.goldReward}, EXP {currentMonster.expReward} 획득.");
        }

        [ContextMenu("몬스터 즉시 처치")]
        public void TestKillMonster() => KillMonster();
    }
}