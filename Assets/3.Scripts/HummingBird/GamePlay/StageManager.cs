using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Bird.Idle.Data;
using Bird.Idle.Core;

namespace Bird.Idle.Gameplay
{
    /// <summary>
    /// 플레이어의 현재 스테이지를 관리하고 스테이지 진행 로직을 제어
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        [Header("Data References")]
        [SerializeField] private AssetLabelReference stageDataLabel;
        
        [Header("Runtime State")]
        [SerializeField] private int currentStageID = 1;
        [SerializeField] private int currentKillCount = 0;

        private Dictionary<int, StageData> stageDataDictionary = new Dictionary<int, StageData>();
        private StageData currentStageData;
        
        private TaskCompletionSource<bool> dataLoadTCS = new TaskCompletionSource<bool>();
        
        public Action<int, int, int> OnStageProgressChanged;
        public Action<int> OnStageChanged;
        
        public Task WaitForDataLoad() => dataLoadTCS.Task;
        public int CurrentStageID => currentStageID;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadStageDataAsync();
        }
        
        /// <summary>
        /// GameManager에서 로드된 데이터를 받아 스테이지 상태를 초기화
        /// </summary>
        public void Initialize(GameSaveData data)
        {
            currentStageID = data.CurrentStageID;
            currentKillCount = data.CurrentKillCount;
            
            SetCurrentStage(currentStageID, currentKillCount);
            
            OnStageProgressChanged?.Invoke(currentKillCount, currentStageData.MonsterKillCountRequired, currentStageID);
            
            Debug.Log($"[StageManager] 스테이지 데이터 로드 완료. Stage ID: {currentStageID}, Kill Count: {currentKillCount}");
        }
        
        /// <summary>
        /// DataManager에 저장할 현재 스테이지 데이터를 GameSaveData에 추가
        /// </summary>
        public void CollectSaveData(GameSaveData data)
        {
            data.CurrentStageID = currentStageID;
            data.CurrentKillCount = currentKillCount;
        }

        private async void LoadStageDataAsync()
        {
            AsyncOperationHandle<IList<StageData>> handle = Addressables.LoadAssetsAsync<StageData>(stageDataLabel, null);

            await handle.Task; 
        
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var data in handle.Result)
                {
                    stageDataDictionary.Add(data.StageID, data);
                }
                Debug.Log($"[StageManager] StageData 로드 완료! (총 {stageDataDictionary.Count}개 스테이지)");
                
                SetCurrentStage(currentStageID, currentKillCount);
                dataLoadTCS.SetResult(true);
            }
            else
            {
                Debug.LogError($"[StageManager] StageData 로드 실패: {handle.OperationException}");
                dataLoadTCS.SetResult(false);
            }
        }
        
        /// <summary>
        /// 현재 스테이지를 설정하고 EnemyManager에 새 정보를 전달
        /// </summary>
        public void SetCurrentStage(int stageID, int killCount)
        {
            if (stageDataDictionary.TryGetValue(stageID, out StageData newStageData))
            {
                OnStageChanged?.Invoke(stageID);
                
                currentStageID = stageID;
                currentStageData = newStageData;
                currentKillCount = killCount;
                
                OnStageProgressChanged?.Invoke(currentKillCount, newStageData.MonsterKillCountRequired, stageID);

                Debug.Log($"[StageManager] 현재 스테이지: {newStageData.StageName} (ID: {stageID})");

                EnemyManager.Instance.UpdateStageData(currentStageData);
            }
            else
            {
                Debug.LogError($"[StageManager] StageID {stageID} 데이터가 없습니다. (최대 레벨 도달)");
            }
        }

        /// <summary>
        /// 몬스터 처치 시 호출되며, 다음 스테이지 진입 조건을 검사
        /// </summary>
        public void OnMonsterKilled()
        {
            if (currentStageData == null) return;

            currentKillCount++;

            OnStageProgressChanged?.Invoke(currentKillCount, currentStageData.MonsterKillCountRequired, currentStageID);
            
            if (currentKillCount >= currentStageData.MonsterKillCountRequired)
            {
                AdvanceToNextStage();
            }
        }

        /// <summary>
        /// 다음 스테이지 또는 보스전으로 진입
        /// </summary>
        private void AdvanceToNextStage()
        {
            if (currentStageData.IsBossStage)
            {
                Debug.Log("[StageManager] 보스 스테이지 클리어! 다음 챕터 해금.");
                SetCurrentStage(currentStageID + 1, 0);
            }
            else
            {
                Debug.Log("[StageManager] 스테이지 클리어. 다음 스테이지로 자동 진행.");
                SetCurrentStage(currentStageID + 1, 0);
            }
        }
    }
}