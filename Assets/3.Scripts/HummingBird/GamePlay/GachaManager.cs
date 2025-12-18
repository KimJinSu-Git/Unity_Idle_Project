using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Bird.Idle.Data;
using Bird.Idle.UI;
using System.Threading.Tasks;
using Bird.Idle.Core;

namespace Bird.Idle.Gameplay
{
    public class GachaManager : MonoBehaviour
    {
        public static GachaManager Instance { get; private set; }

        [Header("Data References")]
        [SerializeField] private GachaLevelData gachaLevelData;

        [Header("Settings")]
        [SerializeField] private int gemCostPerPull = 100;

        private int currentLevel = 1;
        private int currentExp = 0;

        private Dictionary<EquipmentGrade, List<EquipmentData>> accessoryPool = new Dictionary<EquipmentGrade, List<EquipmentData>>();

        public Action<List<EquipmentData>> OnGachaFinished;
        public Action<int, int, int> OnGachaExpChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize(GameSaveData data)
        {
            currentLevel = data.GachaLevel > 0 ? data.GachaLevel : 1;
            currentExp = data.GachaCurrentExp;

            InitializeAccessoryPool();
            
            NotifyStateChanged();
        }

        private void InitializeAccessoryPool()
        {
            accessoryPool.Clear();
            
            var allEquips = EquipmentCollectionManager.Instance.AllEquipmentSO;

            foreach (var item in allEquips.Values)
            {
                if (item.type != EquipmentType.Accessory) continue;

                if (!accessoryPool.ContainsKey(item.grade))
                {
                    accessoryPool.Add(item.grade, new List<EquipmentData>());
                }
                accessoryPool[item.grade].Add(item);
            }
            
            Debug.Log($"[GachaManager] 악세사리 풀 초기화 완료.");
        }

        public void CollectSaveData(GameSaveData data)
        {
            data.GachaLevel = currentLevel;
            data.GachaCurrentExp = currentExp;
        }

        /// <summary>
        /// 뽑기 요청
        /// </summary>
        public void TrySummon(int pullCount)
        {
            long cost = gemCostPerPull * pullCount;
            
            if (CurrencyManager.Instance.GetAmount(CurrencyType.Gem) < cost)
            {
                Debug.LogWarning("젬이 부족합니다.");
                return;
            }

            CurrencyManager.Instance.ChangeCurrency(CurrencyType.Gem, -cost);

            List<EquipmentData> results = new List<EquipmentData>();
            
            for (int i = 0; i < pullCount; i++)
            {
                EquipmentData pickedItem = RollSinglePull();
                if (pickedItem != null)
                {
                    results.Add(pickedItem);
                    EquipmentCollectionManager.Instance.AddItem(pickedItem);
                }
                
                AddExp(1);
            }

            OnGachaFinished?.Invoke(results);
        }

        private EquipmentData RollSinglePull()
        {
            GachaLevelInfo info = gachaLevelData.GetLevelInfo(currentLevel);
            
            float randomVal = UnityEngine.Random.Range(0f, 100f);
            float cumulative = 0f;

            EquipmentGrade selectedGrade = EquipmentGrade.Common;

            if (CheckProbability(ref cumulative, randomVal, info.CommonProb)) selectedGrade = EquipmentGrade.Common;
            else if (CheckProbability(ref cumulative, randomVal, info.RareProb)) selectedGrade = EquipmentGrade.Rare;
            else if (CheckProbability(ref cumulative, randomVal, info.EpicProb)) selectedGrade = EquipmentGrade.Epic;
            else if (CheckProbability(ref cumulative, randomVal, info.LegendaryProb)) selectedGrade = EquipmentGrade.Legendary;

            if (accessoryPool.TryGetValue(selectedGrade, out List<EquipmentData> items) && items.Count > 0)
            {
                int idx = UnityEngine.Random.Range(0, items.Count);
                return items[idx];
            }
            
            if (accessoryPool.TryGetValue(EquipmentGrade.Common, out var commons) && commons.Count > 0)
                 return commons[0];
                 
            return null;
        }

        private bool CheckProbability(ref float cumulative, float randomVal, float prob)
        {
            cumulative += prob;
            return randomVal <= cumulative;
        }

        private void AddExp(int amount)
        {
            GachaLevelInfo info = gachaLevelData.GetLevelInfo(currentLevel);
            
            if (currentLevel >= gachaLevelData.GetMaxLevel()) return;

            currentExp += amount;

            if (currentExp >= info.MaxExp)
            {
                currentExp -= info.MaxExp;
                currentLevel++;
                Debug.Log($"[Gacha] 뽑기 레벨 업! Lv.{currentLevel}");
                
                if(currentLevel < gachaLevelData.GetMaxLevel())
                {
                     
                }
            }
            
            NotifyStateChanged();
        }
        
        private void NotifyStateChanged()
        {
            GachaLevelInfo info = gachaLevelData.GetLevelInfo(currentLevel);
            OnGachaExpChanged?.Invoke(currentLevel, currentExp, info.MaxExp);
        }

        public (int level, int curExp, int maxExp) GetCurrentStatus()
        {
            GachaLevelInfo info = gachaLevelData.GetLevelInfo(currentLevel);
            return (currentLevel, currentExp, info.MaxExp);
        }
    }
}