using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Bird.Idle.Data;
using Bird.Idle.Gameplay;
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
        
        private IEnumerator Start()
        {
            if (StageManager.Instance != null) yield return new WaitUntil(() => StageManager.Instance.WaitForDataLoad().IsCompleted);
            if (EnemyManager.Instance != null) yield return new WaitUntil(() => EnemyManager.Instance.IsDataLoaded);

            CalculateIdleReward();
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

            double effectiveSeconds = Math.Min(totalSeconds, 43200); // 12시간 제한

            if (effectiveSeconds < 60)
            {
                lastExitTime = DateTime.UtcNow;
                return;
            }
            
            int currentStageID = StageManager.Instance.CurrentStageID;
            StageData stageData = StageManager.Instance.GetStageData(currentStageID);

            if (stageData == null) return;
            long totalGold = 0;
            long totalExp = 0;
            Dictionary<EquipmentData, int> acquiredItems = new Dictionary<EquipmentData, int>();
            
            float secondsPerKill = 10.0f; // TODO ::: 평균 사냥 속도 => 나중에 Player의 스탯에 맞춰야 함, 안 그러면 Player 스탯이 딸린데도 10초마다 원콤내는 방치 계산이 되어버림.
            
            int totalKills = (int)(effectiveSeconds / secondsPerKill);
            for (int i = 0; i < totalKills; i++)
            {
                if (stageData.MonsterIDs.Count == 0) break;
            
                int randomIdx = UnityEngine.Random.Range(0, stageData.MonsterIDs.Count);
                int monsterID = stageData.MonsterIDs[randomIdx];
            
                MonsterData mobData = EnemyManager.Instance.GetMonsterData(monsterID);
            
                if (mobData != null)
                {
                    totalGold += (long)(mobData.goldReward * stageData.GoldRewardMultiplier);
                    totalExp += (long)(mobData.expReward * stageData.ExpRewardMultiplier);

                    foreach (var drop in mobData.dropTable)
                    {
                        if (UnityEngine.Random.value <= drop.dropRate)
                        {
                            if (drop.itemSO != null)
                            {
                                if (acquiredItems.ContainsKey(drop.itemSO)) acquiredItems[drop.itemSO]++;
                                else acquiredItems.Add(drop.itemSO, 1);
                            }
                        }
                    }
                }
            }
            
            CurrencyManager.Instance.ChangeCurrency(CurrencyType.Gold, totalGold);
            if (CharacterManager.Instance != null) CharacterManager.Instance.GainExperience(totalExp);
            
            long totalMasuk = 0;

            foreach (var itemKvp in acquiredItems)
            {
                for(int k=0; k < itemKvp.Value; k++)
                {
                    totalMasuk += EquipmentCollectionManager.Instance.AddItem(itemKvp.Key);
                }
            }

            AFKRewardPopup popup = FindObjectOfType<AFKRewardPopup>(true);
            if (popup != null)
            {
                popup.Show(idleDuration, totalGold, totalMasuk, acquiredItems); 
            }

            lastExitTime = DateTime.UtcNow;
        }
        
        /// <summary>
        /// 저장된 게임 데이터를 완전히 삭제합니다. (테스트/초기화 용도)
        /// </summary>
        public void ResetGameData()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.IsResetting = true;
            }
            
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Application.Quit();
            }
            else
            {
                Debug.LogWarning("[DataManager] 삭제할 저장 파일이 존재하지 않습니다.");
            }
        }
        public void OnResetButtonClicked() => ResetGameData();
    }
}