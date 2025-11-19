using System;
using System.Collections.Generic;
using UnityEngine;
using Bird.Idle.Data;
using Bird.Idle.Core;

namespace Bird.Idle.Gameplay
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        private Dictionary<int, QuestData> allQuests = new Dictionary<int, QuestData>();
        private Dictionary<int, QuestProgress> userProgress = new Dictionary<int, QuestProgress>();

        public Action OnQuestProgressUpdated;
        
        /// <summary>
        /// GameManager에서 로드된 데이터를 받아 퀘스트 상태를 초기화
        /// </summary>
        public void Initialize(List<QuestProgress> loadedProgress)
        {
            userProgress.Clear();
            
            if (loadedProgress == null) return;
            
            foreach (var progress in loadedProgress)
            {
                userProgress.Add(progress.questID, progress);
            }
            
            EnsureAllActiveQuestsExist();
            
            SubscribeToEvents();

            Debug.Log($"[QuestManager] 퀘스트 데이터 로드 완료. 복원된 진행 상황: {loadedProgress.Count}개");
            OnQuestProgressUpdated?.Invoke();
        }
        
        /// <summary>
        /// 데이터에 저장되지 않은 신규 퀘스트를 userProgress 맵에 추가
        /// </summary>
        private void EnsureAllActiveQuestsExist()
        {
            // TODO: LoadAllQuestDataAsync가 완료되었다고 가정
            
            foreach (var kvp in allQuests)
            {
                int questID = kvp.Key;
                if (!userProgress.ContainsKey(questID))
                {
                    // 신규 퀘스트 => 기본값
                    userProgress.Add(questID, new QuestProgress { questID = questID, currentValue = 0, rewardsClaimed = 0 });
                }
            }
        }
        
        /// <summary>
        /// DataManager에 저장할 현재 퀘스트 진행 데이터를 수집
        /// </summary>
        public void CollectSaveData(GameSaveData data)
        {
            data.QuestProgressList = new List<QuestProgress>(userProgress.Values);
        }
        
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // TODO: LoadAllQuestDataAsync(); 호출 필요
        }

        /// <summary>
        /// 모든 Manager의 이벤트를 구독하여 퀘스트 진행 상황을 업데이트
        /// </summary>
        private void SubscribeToEvents()
        {
            if (StageManager.Instance != null)
            {
                StageManager.Instance.OnMonsterKilledGlobal += HandleMonsterDefeat;
            }
            // TODO: CharacterManager.OnLevelUp 등 다른 이벤트 구독
        }

        public void HandleMonsterDefeat()
        {
            UpdateProgressByCondition(QuestType.DefeatMonsterCount, 1);
        }

        /// <summary>
        /// 특정 퀘스트 타입의 현재 진행 값을 업데이트
        /// </summary>
        public void UpdateProgressByCondition(QuestType type, long amount)
        {
            foreach (var kvp in allQuests)
            {
                QuestData data = kvp.Value;
                if (data.type != type) continue;

                if (!userProgress.TryGetValue(data.questID, out QuestProgress progress))
                {
                    // 진행 데이터가 없으면 새로 생성
                    progress = new QuestProgress { questID = data.questID };
                    userProgress.Add(data.questID, progress);
                }

                if (!progress.isCompleted) // 일일/업적 퀘스트는 완료 후 업데이트 중지
                {
                    progress.currentValue += amount;
                    RecalculateRewards(progress, data);
                    OnQuestProgressUpdated?.Invoke();
                }
            }
        }
        
        /// <summary>
        /// 반복 퀘스트의 보상 수령 가능 횟수를 계산
        /// </summary>
        private void RecalculateRewards(QuestProgress progress, QuestData data)
        {
            if (data.isRepeatable)
            {
                long timesCompleted = progress.currentValue / data.targetValue;
                
                progress.rewardsClaimed = (int)timesCompleted;
            } 
            else
            {
                if (progress.currentValue >= data.targetValue)
                {
                    progress.isCompleted = true;
                    progress.rewardsClaimed = 1;
                }
            }
        }

        /// <summary>
        /// 퀘스트 보상을 수령하고 진행 상태를 업데이트
        /// </summary>
        public void ClaimReward(int questID)
        {
            if (!allQuests.TryGetValue(questID, out QuestData data) || 
                !userProgress.TryGetValue(questID, out QuestProgress progress) || 
                progress.rewardsClaimed <= 0)
            {
                Debug.LogWarning("[QuestManager] 보상 수령 불가: 퀘스트 ID가 유효하지 않거나 수령 횟수가 0입니다.");
                return;
            }

            // 재화 지급
            CurrencyManager.Instance.ChangeCurrency(data.rewardType, data.rewardAmount * progress.rewardsClaimed);
            
            // 진행 상태 업데이트
            if (data.isRepeatable)
            {
                progress.currentValue -= progress.rewardsClaimed * data.targetValue;
                progress.rewardsClaimed = 0;
            }
            else
            {
                progress.rewardsClaimed = 0;
                // 일일 퀘스트의 경우 다음 날 리셋, 업적은 완료 상태 유지
            }

            OnQuestProgressUpdated?.Invoke();
        }
    }
}