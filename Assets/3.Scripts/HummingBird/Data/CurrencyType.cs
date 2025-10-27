using System;
using UnityEngine;

namespace Bird.Idle.Data
{
    /// <summary>
    /// 게임 내에서 사용되는 재화의 종류
    /// </summary>
    public enum CurrencyType
    {
        Gold, // 기본 재화 (몬스터 처치, 장비 판매)
        Masuk, // 마석 (보스 처치, 희귀 재화)
        SoulFragment, // 영혼 조각
        Gem // 프리미엄 재화
    }
    
    [Serializable]
    public struct CurrencyData
    {
        public CurrencyType currencyType;
        public long amount;

        // 간결성을 위해 표현식 본문 멤버(Expression-bodied member)를 사용합니다.
        public CurrencyData(CurrencyType type, long value)
        {
            currencyType = type;
            amount = value;
        }
    }
}
