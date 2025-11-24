using System;
using System.Collections.Generic;

namespace Bird.Idle.Data
{
    [Serializable]
    public class GameSaveData
    {
        public long LastExitTimeTicks; // 마지막 접속 시간
        
        public long GoldAmount; // Gold 재화
        public long GemAmount; // Gem(보석) 재화
        
        public int PlayerLevel; // Player 캐릭터 레벨
        public float BaseAttackPower; // 기본 공격력
        public float BaseMaxHealth; // 기본 체력
        public float PermanentAttackBonus; // 추가된 공격력
        public float PermanentHealthBonus; // 추가된 체력
        
        public long CurrentEXP; // 현재 Exp
        public int AvailableStatPoints; // Stat Point
        
        // 임시 스탯들
        public int Strength; // 힘
        public int Dexterity; // 민첩
        public int Intelligence; // 지능
        public int Luck; // 행운
        
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
            
            CurrentEXP = 0;
            AvailableStatPoints = 0;
            Strength = 1;
            Dexterity = 1;
            Intelligence = 1;
            Luck = 1;
            
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