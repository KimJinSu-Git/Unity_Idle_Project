using UnityEngine;

namespace Bird.Idle.Utils
{
    /// <summary>
    /// 방치형 게임의 큰 수 표기를 담당하는 유틸리티 클래스
    /// </summary>
    public static class BigNumberFormatter
    {
        // 알파벳 표기 방식
        private static readonly string[] AlphabetUnits =
        {
            "", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", 
            "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
        };

        /// <summary>
        /// double 값을 방치형 포맷 문자열로 변환(알파벳 표기)
        /// </summary>
        public static string Format(double value)
        {
            // 1,000 미만의 수는 소수점 없이 그대로 쉼표만 찍어서 표기
            if (value < 1000d)
            {
                return System.Math.Floor(value).ToString("N0");
            }
            
            string[] currentUnits = AlphabetUnits;
            int unitIndex = 0;
            
            // 값이 1,000 이상이고 단위 배열의 끝에 도달하지 않은 동안 반복해서 1,000으로 나눔
            while (value >= 1000d && unitIndex < currentUnits.Length - 1)
            {
                value /= 1000d;
                unitIndex++;
            }
            
            // 소수점 둘째 자리까지 표기 (1.23A, 45.6B)
            // 소수점 아래가 0이면 생략됩니다 (0.## 포맷)
            return value.ToString("0.##") + currentUnits[unitIndex];
        }

        /// <summary>
        /// long 값을 방치형 포맷 문자열로 변환(알파벳 표기)
        /// </summary>
        public static string Format(long value)
        {
            return Format((double)value);
        }
    }

}
