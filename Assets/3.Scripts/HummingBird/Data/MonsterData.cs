using System.Collections.Generic;
using UnityEngine;

namespace Bird.Idle.Data
{
    [System.Serializable]
    public struct DropItem
    {
        public EquipmentData itemSO; // 드롭될 장비 SO
        [Range(0f, 1f)]
        public float dropRate;      // 드롭 확률
    }
    
    /// <summary>
    /// 개별 몬스터의 기본 스탯과 보상 정보를 정의하는 Scriptable Object.
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterData", menuName = "Bird/Monster Data")]
    public class MonsterData : ScriptableObject
    {
        public int monsterID;
        public string monsterName;
        public float baseHealth;
        public float baseDamage;
        public long goldReward;
        public long expReward;
        
        [Header("드롭 설정")]
        public List<DropItem> dropTable = new List<DropItem>();
        
        [Header("시각적 요소")]
        // public UnityEngine.AddressableAssets.AssetReferenceT<GameObject> prefabReference;
        public string prefabAddress;
    }
}