using UnityEngine;
using System.Collections.Generic;
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
                
                SetCurrentStage(currentStageID);
            }
            else
            {
                Debug.LogError($"[StageManager] StageData 로드 실패: {handle.OperationException}");
            }
        }
        
        /// <summary>
        /// 현재 스테이지를 설정하고 EnemyManager에 새 정보를 전달
        /// </summary>
        public void SetCurrentStage(int stageID)
        {
            if (stageDataDictionary.TryGetValue(stageID, out StageData newStageData))
            {
                currentStageID = stageID;
                currentStageData = newStageData;
                currentKillCount = 0;

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

            if (currentKillCount >= currentStageData.MonsterKillCountRequired)
            {
                AdvanceToNextStage();
            }
            
            // TODO: UI 업데이트 (현재 킬 카운트 / 필요 킬 카운트)
        }

        /// <summary>
        /// 다음 스테이지 또는 보스전으로 진입
        /// </summary>
        private void AdvanceToNextStage()
        {
            if (currentStageData.IsBossStage)
            {
                Debug.Log("[StageManager] 보스 스테이지 클리어! 다음 챕터 해금.");
                SetCurrentStage(currentStageID + 1);
            }
            else
            {
                Debug.Log("[StageManager] 스테이지 클리어. 다음 스테이지로 자동 진행.");
                SetCurrentStage(currentStageID + 1);
            }
        }
    }
}