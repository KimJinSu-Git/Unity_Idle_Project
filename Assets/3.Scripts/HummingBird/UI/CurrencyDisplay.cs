using UnityEngine;
using Bird.Idle.Core;
using Bird.Idle.Data;
using System.Collections.Generic;
using TMPro;

namespace Bird.Idle.UI
{
    /// <summary>
    /// 핵심 재화의 수량을 UI에 표시하고, CurrencyManager의 이벤트를 구독
    /// </summary>
    public class CurrencyDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI goldText; // 골드 표시
        [SerializeField] private TextMeshProUGUI gemText;  // 젬 표시

        // 재화 타입별 컴포넌트를 관리
        private Dictionary<CurrencyType, TextMeshProUGUI> currencyUIMap = new Dictionary<CurrencyType, TextMeshProUGUI>();
        
        private CurrencyManager currencyManager;

        private void Awake()
        {
            currencyManager = CurrencyManager.Instance;
            
            currencyUIMap.Add(CurrencyType.Gold, goldText);
            currencyUIMap.Add(CurrencyType.Gem, gemText);
            // TODO: 다른 재화 타입이 추가 되면 여기에 추가
        }

        private void OnEnable()
        {
            if (currencyManager != null)
            {
                // 재화 변경 이벤트 구독
                currencyManager.OnCurrencyChanged += UpdateCurrencyUI;
                
                InitializeDisplay();
            }
        }

        private void OnDisable()
        {
            if (currencyManager != null)
            {
                // 오브젝트 비활성화 시 구독 해제
                currencyManager.OnCurrencyChanged -= UpdateCurrencyUI;
            }
        }
        
        /// <summary>
        /// 초기 로드 시 모든 재화를 표시
        /// </summary>
        private void InitializeDisplay()
        {
            foreach (var pair in currencyUIMap)
            {
                long amount = currencyManager.GetAmount(pair.Key);
                UpdateCurrencyUI(pair.Key, amount);
            }
        }

        /// <summary>
        /// UI 업데이트
        /// </summary>
        /// <param name="type">변경이 발생한 재화 타입</param>
        /// <param name="newAmount">새로운 재화 수량</param>
        private void UpdateCurrencyUI(CurrencyType type, long newAmount)
        {
            if (currencyUIMap.ContainsKey(type) && currencyUIMap[type] != null)
            {
                string formattedAmount = FormatNumber(newAmount);
                currencyUIMap[type].text = formattedAmount;
            }
        }
        
        private string FormatNumber(long amount)
        {
            return amount.ToString("N0"); 
        }
    }
}