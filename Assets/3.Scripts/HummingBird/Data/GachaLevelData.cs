using UnityEngine;
using System.Collections.Generic;
using Bird.Idle.Data;

namespace Bird.Idle.Data
{
    [System.Serializable]
    public struct GachaLevelInfo
    {
        public int Level;
        public int MaxExp;
        
        [Header("Probabilities")]
        [Range(0, 100)] public float CommonProb;
        [Range(0, 100)] public float RareProb;
        [Range(0, 100)] public float EpicProb;
        [Range(0, 100)] public float LegendaryProb;
    }

    [CreateAssetMenu(fileName = "GachaLevelData", menuName = "Bird/Gacha Level Data")]
    public class GachaLevelData : ScriptableObject
    {
        public List<GachaLevelInfo> LevelTable;

        public GachaLevelInfo GetLevelInfo(int level)
        {
            int index = Mathf.Clamp(level - 1, 0, LevelTable.Count - 1);
            return LevelTable[index];
        }

        public int GetMaxLevel() => LevelTable.Count;
    }
}