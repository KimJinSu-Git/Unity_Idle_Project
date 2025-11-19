using System;
using System.Collections.Generic;

namespace Bird.Idle.Data
{
    [Serializable]
    public class GameSaveData
    {
        public long LastExitTimeTicks;
        
        public long GoldAmount;
        public long GemAmount;
        
        public int PlayerLevel;
        public float BaseAttackPower;
        public float BaseMaxHealth;
        public float PermanentAttackBonus;
        public float PermanentHealthBonus;
        
        public int CurrentStageID; 
        public int CurrentKillCount;
        
        public List<QuestProgress> QuestProgressList;
        
        public List<CollectionEntry> CollectionEntries;
        
        public EquipSaveData EquippedItems; 
        
        public Dictionary<EquipmentType, int> SlotLevels; 

        public GameSaveData()
        {
            PlayerLevel = 1; 
            BaseAttackPower = 10f;  
            BaseMaxHealth = 100f;
            CurrentStageID = 1; 
            CurrentKillCount = 0;
            
            QuestProgressList = new List<QuestProgress>();
            CollectionEntries = new List<CollectionEntry>();
            EquippedItems = new EquipSaveData();
            SlotLevels = new Dictionary<EquipmentType, int>();
        }
    }
    
    [Serializable]
    public class EquipSaveData
    {
        public int WeaponID;
        public int ArmorID;
        public int AccessoryID;
    }
    
    [Serializable]
    public class QuestProgress
    {
        public int questID;
        public long currentValue;    // 현재까지 누적된 값
        public int rewardsClaimed;   // 수령 가능한 보상 횟수
        public bool isCompleted;     // 일일/업적 퀘스트의 완료 여부
    }
}