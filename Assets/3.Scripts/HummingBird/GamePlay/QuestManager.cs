using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Bird.Idle.Data;
using Bird.Idle.Core;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

namespace Bird.Idle.Gameplay
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [Header("Data References")]
        [SerializeField] private AssetLabelReference questDataLabel;
        
        private Dictionary<int, QuestData> allQuests = new Dictionary<int, QuestData>();
        private Dictionary<int, QuestProgress> userProgress = new Dictionary<int, QuestProgress>();
        
        private List<QuestData> mainQuestSequence = new List<QuestData>();
        
        public QuestData CurrentMainQuest { get; private set; }

        public Action OnQuestProgressUpdated; // 전체 갱신
        public Action OnMainQuestChanged; // 나침반 퀘스트 변경 알림
        
        private TaskCompletionSource<bool> dataLoadTCS = new TaskCompletionSource<bool>();
        public Task WaitForDataLoad() => dataLoadTCS.Task;

        public QuestProgress GetQuestProgress(int id)
        {
            return userProgress[id];
        }
        
        public void Initialize(List<QuestProgress> loadedProgress)
        {
            userProgress.Clear();
            if (loadedProgress != null)
            {
                foreach (var p in loadedProgress) userProgress[p.questID] = p;
            }

            EnsureAllActiveQuestsExist();

            UpdateCurrentMainQuest();

            SubscribeToEvents();
            
            OnQuestProgressUpdated?.Invoke();
            OnMainQuestChanged?.Invoke();
        }
        
        private QuestProgress GetOrAmountProgress(int questID)
        {
            if (!userProgress.TryGetValue(questID, out var progress))
            {
                progress = new QuestProgress { questID = questID, currentValue = 0, rewardsClaimed = 0 };
                userProgress.Add(questID, progress);
            }
            return progress;
        }
        
        private void UpdateCurrentMainQuest()
        {
            CurrentMainQuest = null;

            foreach (var quest in mainQuestSequence)
            {
                var progress = GetOrAmountProgress(quest.questID);
                if (!progress.isCompleted)
                {
                    CurrentMainQuest = quest;
                    
                    SyncQuestState(quest, progress);
                    
                    break;
                }
            }
            
            OnMainQuestChanged?.Invoke();
        }
        
        private void SyncQuestState(QuestData quest, QuestProgress progress)
        {
            long currentValue = 0;
            bool needUpdate = false;

            switch (quest.type)
            {
                case QuestType.LevelUpCharacter:
                    if (CharacterManager.Instance != null)
                    {
                        currentValue = CharacterManager.Instance.CharacterLevel; 
                        needUpdate = true;
                    }
                    break;
                case QuestType.ReachStage:
                    if (StageManager.Instance != null)
                    {
                        currentValue = StageManager.Instance.CurrentStageID;
                        needUpdate = true;
                    }
                    break;
                // TODO :: 슬롯 강화 레벨 등
            }

            if (needUpdate)
            {
                if (currentValue > progress.currentValue)
                {
                    progress.currentValue = currentValue;
            
                    OnQuestProgressUpdated?.Invoke(); 
                }
            }
        }
        
        /// <summary>
        /// 데이터에 저장되지 않은 신규 퀘스트를 userProgress 딕셔너리에 추가
        /// </summary>
        private void EnsureAllActiveQuestsExist()
        {
            foreach (var kvp in allQuests)
            {
                int questID = kvp.Key;
                if (!userProgress.ContainsKey(questID))
                {
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
            
            LoadAllQuestDataAsync();
        }
        
        
        private void OnDestroy()
        {
            if (SlotManager.Instance != null)
            {
                SlotManager.Instance.OnSlotEnhanced -= HandleSlotEnhance;
            }
        }
        
        private async void LoadAllQuestDataAsync()
        {
            var handle = Addressables.LoadAssetsAsync<QuestData>(questDataLabel, null);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var data in handle.Result)
                {
                    if (!allQuests.ContainsKey(data.questID))
                    {
                        allQuests.Add(data.questID, data);
                    }
                }

                mainQuestSequence = allQuests.Values
                    .Where(q => q.category == QuestCategory.Main)
                    .OrderBy(q => q.questID)
                    .ToList();
                
                dataLoadTCS.SetResult(true);
            }
            else
            {
                dataLoadTCS.SetResult(false);
            }
        }

        /// <summary>
        /// 모든 Manager의 이벤트를 구독하여 퀘스트 진행 상황을 업데이트
        /// </summary>
        private void SubscribeToEvents()
        {
            if (StageManager.Instance != null)
                StageManager.Instance.OnMonsterKilledGlobal += HandleMonsterDefeat;
            
            if (CharacterManager.Instance != null)
                CharacterManager.Instance.OnLevelUp += HandleLevelUp;
            
            if (GachaManager.Instance != null)
                GachaManager.Instance.OnGachaFinished += HandleGachaPerformed;
            
            if (SlotManager.Instance != null)
                SlotManager.Instance.OnSlotEnhanced += HandleSlotEnhance;
        }
        
        private void HandleMonsterDefeat() => UpdateProgressByCondition(QuestType.DefeatMonsterCount, 1);
        private void HandleLevelUp(int level) => UpdateProgressByCondition(QuestType.LevelUpCharacter, level, true);
        private void HandleGachaPerformed(List<EquipmentData> items)
        {
            if (items != null && items.Count > 0)
            {
                UpdateProgressByCondition(QuestType.PerformGacha, items.Count);
            }
        }
        private void HandleSlotEnhance(EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.Weapon:
                    UpdateProgressByCondition(QuestType.EnhanceSlot_Weapon, 1);
                    break;
                case EquipmentType.Armor:
                    UpdateProgressByCondition(QuestType.EnhanceSlot_Armor, 1);
                    break;
                case EquipmentType.Accessory:
                    UpdateProgressByCondition(QuestType.EnhanceSlot_Accessory, 1);
                    break;
            }
        }

        /// <summary>
        /// 특정 퀘스트 타입의 현재 진행 값을 업데이트
        /// </summary>
        public void UpdateProgressByCondition(QuestType type, long amount, bool isSet = false)
        {
            bool changed = false;

            if (CurrentMainQuest != null && CurrentMainQuest.type == type)
            {
                var progress = GetOrAmountProgress(CurrentMainQuest.questID);
                
                if (!progress.isCompleted && progress.rewardsClaimed == 0)
                {
                    if (isSet) progress.currentValue = amount;
                    else progress.currentValue += amount;

                    changed = true;
                }
            }

            foreach (var kvp in allQuests)
            {
                var data = kvp.Value;
                if (data.category == QuestCategory.Repeatable && data.type == type)
                {
                    var progress = GetOrAmountProgress(data.questID);
                    
                    if (isSet) progress.currentValue = amount;
                    else progress.currentValue += amount;

                    RecalculateRepeatableRewards(progress, data);
                    changed = true;
                }
            }

            if (changed) OnQuestProgressUpdated?.Invoke();
        }
        
        private void RecalculateRepeatableRewards(QuestProgress progress, QuestData data)
        {
            if (data.targetValue <= 0) return;
            long count = progress.currentValue / data.targetValue;
            progress.rewardsClaimed = (int)count;
        }

        /// <summary>
        /// 퀘스트 보상을 수령하고 진행 상태를 업데이트
        /// </summary>
        public void ClaimReward(int questID)
        {
            if (!allQuests.TryGetValue(questID, out QuestData data) || !userProgress.TryGetValue(questID, out QuestProgress progress)) return;

            if (data.category == QuestCategory.Main)
            {
                if (progress.currentValue >= data.targetValue && !progress.isCompleted)
                {
                    CurrencyManager.Instance.ChangeCurrency(data.rewardType, data.rewardAmount);
                    
                    progress.isCompleted = true;
                    progress.rewardsClaimed = 1;
                    
                    UpdateCurrentMainQuest();
                    
                    OnQuestProgressUpdated?.Invoke();
                }
            }
            // --- 반복 퀘스트 ---
            else if (data.category == QuestCategory.Repeatable)
            {
                if (progress.rewardsClaimed > 0)
                {
                    long totalReward = data.rewardAmount * progress.rewardsClaimed;
                    CurrencyManager.Instance.ChangeCurrency(data.rewardType, totalReward);

                    // 잔여량 유지 (465/100 -> 65/100)
                    progress.currentValue %= data.targetValue;
                    progress.rewardsClaimed = 0;

                    OnQuestProgressUpdated?.Invoke();
                }
            }
        }
        
        public List<QuestData> GetRepeatableQuests()
        {
            return allQuests.Values
                .Where(q => q.category == QuestCategory.Repeatable)
                .OrderBy(q => q.questID)
                .ToList();
        }
    }
}