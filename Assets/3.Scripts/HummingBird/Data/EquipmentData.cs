using UnityEngine;

namespace Bird.Idle.Data
{
    // 장비의 타입을 구분하기 위한 Enum 정의
    public enum EquipmentType { Weapon, Armor, Accessory }
    public enum EquipmentGrade { Common, Rare, Epic, Legendary }

    /// <summary>
    /// 개별 장비의 스탯 및 정보를 정의하는 Scriptable Object.
    /// </summary>
    [CreateAssetMenu(fileName = "EquipData", menuName = "Bird/Equipment Data")]
    public class EquipmentData : ScriptableObject
    {
        public int equipID;
        public string equipName;
        public EquipmentType type;
        public EquipmentGrade grade;
        
        public float attackBonus;
        public float healthBonus;
        public long sellPrice;
        
        public string iconAddress; 
    }
}