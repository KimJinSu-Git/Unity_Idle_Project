using System;
using UnityEngine;
using System.Collections.Generic;

namespace Bird.Idle.Data
{
    public enum QuestType
    {
        ClearStage,          // 특정 스테이지 클리어
        DefeatMonsterCount,  // 몬스터 처치 수
        LevelUpCharacter,    // 캐릭터 레벨
        EnhanceSlot,         // 슬롯 강화 레벨
        UpgradeCollection    // 장비 컬렉션 강화 레벨
    }
    
    [CreateAssetMenu(fileName = "QuestData_", menuName = "Bird/Quest Data")]
    public class QuestData : ScriptableObject
    {
        public int questID;
        public string questName;
        public string description;
        
        public QuestType type;
        public bool isRepeatable; // 반복 퀘스트 여부
        
        [Header("목표 설정")]
        public long targetValue; // 달성해야 할 목표 값
        
        // TODO: 특정 장비 타입/ID가 필요하다면 추가 필드 필요

        [Header("보상")]
        public CurrencyType rewardType;
        public long rewardAmount;
    }
}