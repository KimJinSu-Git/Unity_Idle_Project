using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bird.Idle.UI
{
    public class RewardLogManager : MonoBehaviour
    {
        public static RewardLogManager Instance { get; private set; }
        
        [Header("UI Setup")]
        [SerializeField] private GameObject logItemPrefab;
        [SerializeField] private Transform logContainer; // Vertical Layout Group

        [Header("Resource Icon")] 
        public Sprite expIcon;
        public Sprite goldIcon;
        public Sprite masukIcon;
        
        private Queue<RewardLogItemUI> pool = new Queue<RewardLogItemUI>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 로그를 화면에 띄웁니다
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="message"></param>
        /// <param name="textColor"></param>
        public void ShowLog(Sprite icon, string message, Color textColor)
        {
            RewardLogItemUI logItem;

            // 풀에 남은 UI가 있다면 꺼내쓰기
            if (pool.Count > 0)
            {
                logItem = pool.Dequeue();
                logItem.gameObject.SetActive(true);
            }
            else // 없다면 새로 인스턴스화
            {
                GameObject obj = Instantiate(logItemPrefab, logContainer);
                logItem = obj.GetComponent<RewardLogItemUI>();
            }
            
            // 하이어라키의 가장 맨 아래(최신)으로 이동
            logItem.transform.SetAsLastSibling();
            logItem.Setup(icon, message, textColor);
        }

        /// <summary>
        /// 사용이 끝난 로그 UI를 풀에 반환합니다.
        /// </summary>
        /// <param name="logItem"></param>
        public void ReturnToPool(RewardLogItemUI logItem)
        {
            logItem.gameObject.SetActive(false);
            pool.Enqueue(logItem);
        }
    }
}

