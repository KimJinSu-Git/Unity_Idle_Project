using UnityEngine;
using System.Collections.Generic;

namespace Bird.Idle.Data
{
    /// <summary>
    /// 장비 칸(Slot) 강화에 필요한 정보를 정의하는 SO.
    /// </summary>
    [CreateAssetMenu(fileName = "SlotEnhanceData", menuName = "Bird/Slot Enhance Data")]
    public class SlotEnhanceData : ScriptableObject
    {
        // 장비 타입별 강화 테이블 (무기, 방어구, 악세사리)
        public List<SlotEnhanceTable> TypeTables = new List<SlotEnhanceTable>();

        [System.Serializable]
        public class SlotEnhanceTable
        {
            public EquipmentType Type; // 강화 테이블이 적용될 슬롯 타입
            public List<SlotEnhanceEntry> Entries = new List<SlotEnhanceEntry>();
        }

        [System.Serializable]
        public struct SlotEnhanceEntry
        {
            public int EnhanceLevel;        // 강화 레벨
            public long GoldCost;           // 요구 골드
            public float AttackIncrease;     // 증가하는 기본 공격력
            public float HealthIncrease;     // 증가하는 최대 체력
            public float SuccessRate;        // 성공 확률
        }
        
    }
}