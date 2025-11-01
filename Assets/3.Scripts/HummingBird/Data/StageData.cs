using UnityEngine;
using Bird.Idle.Data;
using System.Collections.Generic;

namespace Bird.Idle.Data
{
    /// <summary>
    /// 스테이지의 기본 정보 및 구성 몬스터 데이터를 정의하는 SO.
    /// </summary>
    [CreateAssetMenu(fileName = "StageData_", menuName = "Bird/Stage Data")]
    public class StageData : ScriptableObject
    {
        public int StageID; 
        public string StageName;
        
        [Header("몬스터 구성")]
        public List<int> MonsterIDs; // 해당 스테이지에 등장할 몬스터 ID 목록
        public int MonsterKillCountRequired = 100; // 다음 스테이지로 넘어가기 위해 필요한 몬스터 처치 수
        
        [Header("보스전 설정")]
        public bool IsBossStage = false;
        public int BossMonsterID = -1; // 보스 몬스터 ID
        
        [Header("보상 배율")]
        public float GoldRewardMultiplier = 1.0f; // 골드 배율
        public float ExpRewardMultiplier = 1.0f;  // 경험치 배율
    }
}