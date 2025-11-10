using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Bird.Idle.Data;
using Bird.Idle.UI;

namespace Bird.Idle.Core
{
    /// <summary>
    /// 게임 데이터의 저장/로드 및 방치 시간을 관리하는 싱글톤 클래스
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }

        private string savePath;
        
        // 마지막 종료 시간
        private DateTime lastExitTime;
        public DateTime LastExitTime => lastExitTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 데이터 저장 경로 설정
            savePath = Application.persistentDataPath + "/gameData.dat";

            LoadGameData(); 
        }

        /// <summary>
        /// 비동기 방식으로 게임 데이터를 저장합니다.
        /// </summary>
        public async Task SaveGameData(GameSaveData data)
        {
            var saveData = data;

            await Task.Run(() =>
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    using (FileStream stream = new FileStream(savePath, FileMode.Create))
                    {
                        formatter.Serialize(stream, saveData);
                    }
                    Debug.Log($"[DataManager] 게임 데이터 저장 완료. 시간: {lastExitTime}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DataManager] 데이터 저장 실패: {e.Message}");
                }
            });
        }

        /// <summary>
        /// 비동기 방식으로 게임 데이터를 로드
        /// </summary>
        public async Task<GameSaveData> LoadGameData()
        {
            if (!File.Exists(savePath))
            {
                Debug.LogWarning("[DataManager] 저장된 파일이 없습니다. 새 게임 시작.");
                lastExitTime = DateTime.UtcNow;
                return new GameSaveData { LastExitTimeTicks = lastExitTime.Ticks };
            }

            GameSaveData loadedData = null;
            await Task.Run(() =>
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    using (FileStream stream = new FileStream(savePath, FileMode.Open))
                    {
                        loadedData = formatter.Deserialize(stream) as GameSaveData;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DataManager] 데이터 로드 실패. 새 게임을 시작합니다. 에러: {e.Message}");
                    loadedData = new GameSaveData();
                }
            });

            // 로드 성공 시 데이터 적용
            if (loadedData != null)
            {
                lastExitTime = new DateTime(loadedData.LastExitTimeTicks, DateTimeKind.Utc);
                Debug.Log($"[DataManager] 데이터 로드 완료. 마지막 종료 시간: {lastExitTime}");
            }
            
            return loadedData;
        }

        // ==================== 방치 보상 로직 ====================

        /// <summary>
        /// 게임 접속 시 오프라인 보상을 계산하고 지급
        /// </summary>
        public void CalculateIdleReward()
        {
            TimeSpan idleDuration = DateTime.UtcNow - lastExitTime;
            double totalSeconds = idleDuration.TotalSeconds;

            // 최대 보상 시간 제한
            const double MAX_IDLE_SECONDS = 43200; 
            double effectiveSeconds = Math.Min(totalSeconds, MAX_IDLE_SECONDS);

            if (effectiveSeconds > 60) // 최소 1분 이상 방치했을 때만 보상 계산
            {
                const int GOLD_PER_SECOND = 10;
                long rewardedGold = (long)(effectiveSeconds * GOLD_PER_SECOND);
                
                CurrencyManager.Instance.ChangeCurrency(Data.CurrencyType.Gold, rewardedGold);
                
                Debug.Log($"[AFK] 방치 시간: {idleDuration.Hours}시간 {idleDuration.Minutes}분. 골드 보상: {rewardedGold}");
                
                AFKRewardPopup popup = FindObjectOfType<AFKRewardPopup>(true); 
                if (popup != null)
                {
                    popup.Show(idleDuration, rewardedGold);
                }
            }
            else
            {
                Debug.Log("방치 시간이 짧아 보상 지급 대상이 아닙니다.");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ResumeGameAfterAFK();
                }
            }
            
            lastExitTime = DateTime.UtcNow;
        }
    }
}