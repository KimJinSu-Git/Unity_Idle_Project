using UnityEngine;

namespace Bird.Idle.Data
{
    /// <summary>
    /// 개별 몬스터의 기본 스탯과 보상 정보를 정의하는 Scriptable Object.
    /// 에셋 생성 메뉴 경로: Assets/Create/Bird/Monster Data
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
        // TODO: 몬스터 모델/프리팹 참조 추가
    }
}