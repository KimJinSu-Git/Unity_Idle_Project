using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Bird.Idle.UI
{
    public class RewardLogManager : MonoBehaviour
    {
        public static RewardLogManager Instance { get; private set; }
        
        [Header("UI Setup")]
        [SerializeField] private GameObject logItemPrefab;
        [SerializeField] private Transform logContainer; // Vertical Layout Group
        [SerializeField] private int maxLogCount = 4;

        [Header("Resource Icon")] 
        public Sprite expIcon;
        public Sprite goldIcon;
        public Sprite masukIcon;
        
        private Queue<RewardLogItemUI> pool = new Queue<RewardLogItemUI>();
        private List<RewardLogItemUI> activeLogs = new List<RewardLogItemUI>();
        
        private Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();

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
        /// Sprite를 직접 받아 화면에 띄우는 ShowLog
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="message"></param>
        /// <param name="textColor"></param>
        public void ShowLog(Sprite icon, string message, Color textColor)
        {
            if (activeLogs.Count >= maxLogCount)
            {
                RewardLogItemUI oldestLog = activeLogs[0];
                oldestLog.ForceClose();
            }
            
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
            
            activeLogs.Add(logItem);
            // 하이어라키의 가장 맨 아래(최신)으로 이동
            logItem.transform.SetAsLastSibling();
            logItem.Setup(icon, message, textColor);
        }
        
        // 어드레서블 주소를 받는 ShowLog
        public async void ShowLog(string iconAddress, string message, Color textColor)
        {
            Sprite loadedIcon = null;

            if (!string.IsNullOrEmpty(iconAddress))
            {
                // 이미 불러온 적이 있는 아이콘이면 캐시에서 바로 꺼냄
                if (iconCache.TryGetValue(iconAddress, out Sprite cachedSprite))
                {
                    loadedIcon = cachedSprite;
                }
                else // 처음 획득한 아이템이라면 어드레서블로 로드하여 캐시에 저장
                {
                    AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(iconAddress);
                    await handle.Task;

                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        loadedIcon = handle.Result;
                        iconCache[iconAddress] = loadedIcon; // 다음 획득 시 빠른 로드를 위해 저장
                    }
                    else
                    {
                        Debug.LogWarning($"[RewardLogManager] 아이콘 로드 실패: {iconAddress}");
                    }
                }
            }

            // 아이콘이 준비되었으므로, 기존의 Sprite용 ShowLog를 호출
            ShowLog(loadedIcon, message, textColor);
        }

        /// <summary>
        /// 사용이 끝난 로그 UI를 풀에 반환합니다.
        /// </summary>
        /// <param name="logItem"></param>
        public void ReturnToPool(RewardLogItemUI logItem)
        {
            if (activeLogs.Contains(logItem))
            {
                activeLogs.Remove(logItem);
            }
            
            logItem.gameObject.SetActive(false);
            pool.Enqueue(logItem);
        }
    }
}

