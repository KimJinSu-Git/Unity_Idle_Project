using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Bird.Idle.Data;
using Bird.Idle.Core;
using Bird.Idle.UI;

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
        
        [Header("Other References")]
        [SerializeField] private UI_ScreenFader uiScreenFader;

        private bool initialStart = true;
        private bool isTransitioning = false;
        
        private int maxReachedStageID;

        private Dictionary<int, StageData> stageDataDictionary = new Dictionary<int, StageData>();
        private StageData currentStageData;
        
        private TaskCompletionSource<bool> dataLoadTCS = new TaskCompletionSource<bool>();
        
        public Action<int, int, int> OnStageProgressChanged;
        public Action<int> OnStageChanged;
        public Action OnMonsterKilledGlobal;
        
        public Task WaitForDataLoad() => dataLoadTCS.Task;
        public int MaxReachedStageID => maxReachedStageID;
        public int CurrentStageID => currentStageID;
        
        public Action<bool> OnFarmingModeChanged;

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
            
            maxReachedStageID = data.MaxReachedStageID;
            if (maxReachedStageID == 0) maxReachedStageID = 1;
            
            SetCurrentStage(currentStageID, currentKillCount);
            
            OnStageProgressChanged?.Invoke(currentKillCount, currentStageData.MonsterKillCountRequired, currentStageID);
        }
        
        /// <summary>
        /// DataManager에 저장할 현재 스테이지 데이터를 GameSaveData에 추가
        /// </summary>
        public void CollectSaveData(GameSaveData data)
        {
            data.CurrentStageID = currentStageID;
            data.CurrentKillCount = currentKillCount;
            data.MaxReachedStageID = maxReachedStageID;
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
                
                // SetCurrentStage(currentStageID, currentKillCount);
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
                EnemyManager.Instance.ClearAllMonsters();
                
                bool stageActuallyChanged = (currentStageID != stageID);
                
                currentStageID = stageID;
                currentStageData = newStageData;
                
                bool isFarmingMode = currentStageID < maxReachedStageID;
                
                if (isFarmingMode)
                {
                    currentKillCount = currentStageData.MonsterKillCountRequired;
                }
                else
                {
                    currentKillCount = killCount;
                    if (currentStageID > maxReachedStageID) maxReachedStageID = currentStageID;
                }
                
                if (stageActuallyChanged || initialStart)
                {
                    initialStart = false;
                }
                
                OnStageChanged?.Invoke(stageID);
                OnStageProgressChanged?.Invoke(currentKillCount, newStageData.MonsterKillCountRequired, stageID);
                
                OnFarmingModeChanged?.Invoke(isFarmingMode);

                EnemyManager.Instance.UpdateStageData(currentStageData, currentKillCount, isFarmingMode);
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
            if (currentStageID < maxReachedStageID)
            {
                OnMonsterKilledGlobal?.Invoke(); // 파밍 모드라면 잡았다는 이벤트 업뎃만
                return;
            }

            currentKillCount++;
            OnMonsterKilledGlobal?.Invoke();
            OnStageProgressChanged?.Invoke(currentKillCount, currentStageData.MonsterKillCountRequired, currentStageID);
            
            if (currentKillCount >= currentStageData.MonsterKillCountRequired)
            {
                StartCoroutine(AdvanceToNextStageCo());
            }
        }

        private IEnumerator AdvanceToNextStageCo()
        {
            yield return new WaitForSeconds(1.5f);
            
            int nextStageID = currentStageID + 1;
            maxReachedStageID = nextStageID; // 최고 기록 갱신

            yield return StartCoroutine(ChangeStageWithEffect(nextStageID, 0f));
        }
        
        public void RequestStageChange(int targetStageID)
        {
            if (isTransitioning) return;
            if (currentStageID == targetStageID) return; 

            if (targetStageID <= maxReachedStageID)
            {
                StartCoroutine(ChangeStageWithEffect(targetStageID));
            }
            else
            {
                if (UI_ToastMessage.Instance != null) 
                    UI_ToastMessage.Instance.Show("Lock Stage..");
            }
        }
        
        private IEnumerator ChangeStageWithEffect(int targetStageID, float startDelay = 0f)
        {
            isTransitioning = true;
            if (startDelay > 0) yield return new WaitForSeconds(startDelay);

            if (uiScreenFader != null)
            {
                yield return uiScreenFader.FadeOut(0.5f);
            }

            SetCurrentStage(targetStageID, 0);

            yield return new WaitForSeconds(0.2f);

            if (uiScreenFader != null)
            {
                yield return uiScreenFader.FadeIn(0.5f);
            }

            isTransitioning = false;
        }
        
        // 최고 Stage로 돌아오기에 사용할 기능
        public void ReturnToMaxStage()
        {
            if (currentStageID != maxReachedStageID)
            {
                SetCurrentStage(maxReachedStageID, 0);
            }
        }
        
        /// <summary>
        /// 스테이지 ID로 StageData를 반환 (방치 보상 계산용)
        /// </summary>
        public StageData GetStageData(int stageID)
        {
            if (stageDataDictionary.TryGetValue(stageID, out StageData data)) return data;
            return null;
        }
    }
}