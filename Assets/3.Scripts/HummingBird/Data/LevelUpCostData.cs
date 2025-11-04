using UnityEngine;
using System.Collections.Generic;

namespace Bird.Idle.Data
{
    /// <summary>
    /// 캐릭터의 레벨별 요구 골드 비용 및 스탯 증가량을 정의하는 Scriptable Object.
    /// 에셋 생성 메뉴 경로: Assets/Create/Bird/Level Data
    /// </summary>
    [CreateAssetMenu(fileName = "LevelUpCostData", menuName = "Bird/Level Up Cost Data")]
    public class LevelUpCostData : ScriptableObject
    {
        public List<LevelEntry> LevelTable = new List<LevelEntry>();

        // 레벨 정보를 담는 구조체
        [System.Serializable]
        public struct LevelEntry
        {
            public int Level;
            public long RequiredGold; // 요구 골드 비용
            public float AttackIncrease; // 해당 레벨에서 증가하는 기본 공격력
            public float HealthIncrease; // 해당 레벨에서 증가하는 최대 체력
        }
        
        /// <summary>
        /// 특정 레벨의 데이터 가져오기
        /// </summary>
        public LevelEntry GetLevelEntry(int level)
        {
            // TODO :: 우선, Find로 찾도록 구성했지만 레벨 데이터의 크기가 커진다면 탐색 시간이 O(n)이므로 Dictionary 캐싱 등으로 변경 필요
            return LevelTable.Find(e => e.Level == level); 
        }
    }
}