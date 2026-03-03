using System;

namespace Bird.Idle.Core
{
    public static class UpgradeCalculator
    {
        /// <summary>
        /// 지수 함수 기반의 업그레이드 비용을 계산
        /// </summary>
        /// <param name="baseCost">1레벨일 때의 기본 비용</param>
        /// <param name="growthFactor">레벨당 증가 배수 (예: 1.07f = 매 레벨 7%씩 증가)</param>
        /// <param name="currentLevel">현재 도달한 레벨</param>
        public static long GetUpgradeCost(long baseCost, float growthFactor, int currentLevel)
        {
            // Math.Pow를 이용해 복리 지수승 계산
            double multiplier = Math.Pow(growthFactor, currentLevel - 1);
            return (long)(baseCost * multiplier);
        }
    }
}