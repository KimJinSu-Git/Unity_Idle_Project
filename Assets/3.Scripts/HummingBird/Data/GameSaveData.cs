using System;
using System.Collections.Generic;

namespace Bird.Idle.Data
{
    [Serializable]
    public class GameSaveData
    {
        public long LastExitTimeTicks;
        
        public long GoldAmount;
        // TODO: public long GemAmount;
        
        public int PlayerLevel;
        public float BaseAttackPower;
        public float BaseMaxHealth;
        public float PermanentAttackBonus;
        public float PermanentHealthBonus;
        
        public List<CollectionEntry> CollectionEntries;
        
        public EquipSaveData EquippedItems; 
        
        public Dictionary<EquipmentType, int> SlotLevels; 

        public GameSaveData()
        {
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
}