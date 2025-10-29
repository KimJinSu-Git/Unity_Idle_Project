using System;
using System.Collections.Generic;
using UnityEngine;
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
        [SerializeField] private List<MonsterData> stageMonsterList;
        
        [SerializeField] private float spawnInterval = 1.0f; // 몬스터 스폰 주기
        // [SerializeField] private long goldPerKill = 100; // 몬스터 처치당 골드
        // [SerializeField] private long expPerKill = 50;  // 몬스터 처치당 경험치
        [SerializeField] private int maxMonsterCount = 5; // 최대 몬스터 수

        private float currentSpawnTime;
        private int currentMonsterCount = 0;

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
            if (currentMonsterCount <= 0 || stageMonsterList.Count == 0) return;
            
            MonsterData currentMonster = stageMonsterList[0];

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