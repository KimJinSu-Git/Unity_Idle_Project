using System;
using System.Collections.Generic;
using UnityEngine;
using Bird.Idle.Data;

namespace Bird.Idle.Core
{
    /// <summary>
    /// 게임 내 모든 재화를 관리하는 싱글톤 클래스
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        private Dictionary<CurrencyType, long> currencyAmounts = new Dictionary<CurrencyType, long>();

        // 재화 변경 시 외부에 알릴 Action
        public Action<CurrencyType, long> OnCurrencyChanged;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeCurrencies();
        }

        private void InitializeCurrencies()
        {
            foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
            {
                currencyAmounts[type] = 0; 
            }
        }
        
        /// <summary>
        /// 데이터 로드 시 골드 데이터를 초기화
        /// </summary>
        public void InitializeGold(long amount) 
        {
            currencyAmounts[CurrencyType.Gold] = amount;
            Debug.Log($"[CurrencyManager] 골드 로드 완료 및 설정: {amount:N0}");
            
            // UI 갱신을 위해 이벤트 호출
            OnCurrencyChanged?.Invoke(CurrencyType.Gold, amount); 
        }
        
        /// <summary>
        /// DataManager에 저장할 현재 재화 데이터를 수집
        /// </summary>
        public void CollectSaveData(GameSaveData data)
        {
            data.GoldAmount = GetAmount(CurrencyType.Gold);
            data.GemAmount = GetAmount(CurrencyType.Gem);
        }

        
        /// <summary>
        /// 특정 재화의 현재 수량 가져오기
        /// </summary>
        /// <param name="type">가져올 재화의 종류</param>
        /// <returns>현재 재화 수량</returns>
        public long GetAmount(CurrencyType type)
        {
            return currencyAmounts.ContainsKey(type) ? currencyAmounts[type] : 0;
        }

        /// <summary>
        /// 재화를 획득하거나 소모
        /// </summary>
        /// <param name="type">재화의 종류</param>
        /// <param name="amount">변경할 수량</param>
        /// <returns>변경 성공 여부 </returns>
        public bool ChangeCurrency(CurrencyType type, long amount)
        {
            long currentAmount = GetAmount(type);
            long newAmount = currentAmount + amount;

            if (newAmount < 0)
            {
                Debug.LogWarning($"재화 소모 실패: {type}의 잔액이 부족합니다. (현재: {currentAmount}, 요청 소모량: {-amount})");
                return false; 
            }

            currencyAmounts[type] = newAmount;

            OnCurrencyChanged?.Invoke(type, newAmount); 

            // Debug.Log($"[Change] {type}이(가) {amount:+#;-#;0} 만큼 변경되었습니다. 현재 수량: {newAmount}");
            return true;
        }

        /// <summary>
        /// 재화를 소모할 수 있는지 검사
        /// </summary>
        public bool CanAfford(CurrencyType type, long cost) => GetAmount(type) >= cost;
    }
}