using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Bird.Idle.Data;
using Bird.Idle.Gameplay;
using Bird.Idle.UI;
using UnityEngine.InputSystem;

namespace Bird.Idle.Core
{
    /// <summary>
    /// 게임의 전역 상태 및 흐름을 제어하는 싱글톤 클래스
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private BattleManager battleManager;
        
        [SerializeField] private GameExitPopup exitPopup;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            Application.targetFrameRate = 60;
            
            battleManager = BattleManager.Instance;
            StartGameFlow();
            
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.OnRequestStageRestart += HandlePlayerDeathAndRestart;
            }
        }
        
        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (exitPopup != null)
                {
                    if (exitPopup.gameObject.activeSelf)
                        exitPopup.gameObject.SetActive(false);
                    else
                        exitPopup.Show();
                }
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                _ = SaveGameOnExitAsync();
            }
        }
        
        private void OnDestroy()
        {
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.OnRequestStageRestart -= HandlePlayerDeathAndRestart;
            }
        }
        
        private void HandlePlayerDeathAndRestart()
        {
            if (battleManager != null)
            {
                battleManager.SetBattleActive(false); 
            }
            
            // TODO ::: 스테이지 재시작 호출
            StartCoroutine(RestartStageAfterDelay(2f));
        }
        
        private IEnumerator RestartStageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.ClearAllMonsters();
            }
            
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.Revive();
            }
            
            if (battleManager != null)
            {
                battleManager.SetBattleActive(true); 
            }
            
            StageManager.Instance.SetCurrentStage(StageManager.Instance.CurrentStageID, 0);
        }
        
        /// <summary>
        /// 게임 시작 시 초기 흐름을 관리
        /// </summary>
        private async void StartGameFlow()
        {
            GameSaveData loadedData = await DataManager.Instance.LoadGameData();
            
            if (EquipmentCollectionManager.Instance != null)
            {
                await EquipmentCollectionManager.Instance.WaitForDataLoad();
            }
            if (SlotManager.Instance != null)
            {
                await SlotManager.Instance.WaitForDataLoad(); 
            }
            if (StageManager.Instance != null)
            {
                await StageManager.Instance.WaitForDataLoad();
            }
            
            ApplyLoadedDataToManagers(loadedData);
            
            EquipPanel equipPanel = FindObjectOfType<EquipPanel>(true);
            if (equipPanel != null)
            {
                equipPanel.InitializeAfterDataLoad(); 
            }
            
            // CalculateIdleReward(loadedData);
            
            // SetBattleState(false);
        }
        
        /// <summary>
        /// 로드된 데이터를 각 관리자에게 전달하고 초기화
        /// </summary>
        private void ApplyLoadedDataToManagers(GameSaveData data)
        {
            CurrencyManager.Instance.InitializeAllCurrencies(data.GoldAmount, data.GemAmount, data.MasukAmount, data.SoulFragmentAmount);
            CharacterManager.Instance.Initialize(data);
            StageManager.Instance.Initialize(data);
            
            Dictionary<int, EquipmentData> allEquipmentMap = EquipmentCollectionManager.Instance?.AllEquipmentSO;
            
            EquipmentCollectionManager.Instance.Initialize(data.CollectionEntries);
            InventoryManager.Instance.Initialize(data.EquippedItems, allEquipmentMap);
            SlotManager.Instance.Initialize(data.SlotLevels);
            GachaManager.Instance.Initialize(data);
        }
        
        public async Task SaveGameOnExitAsync()
        {
            GameSaveData data = new GameSaveData();
        
            if (CharacterManager.Instance != null) CharacterManager.Instance.CollectSaveData(data);
            if (CurrencyManager.Instance != null) CurrencyManager.Instance.CollectSaveData(data);
            if (EquipmentCollectionManager.Instance != null) EquipmentCollectionManager.Instance.CollectSaveData(data);
            if (InventoryManager.Instance != null) InventoryManager.Instance.CollectSaveData(data);
            if (SlotManager.Instance != null) SlotManager.Instance.CollectSaveData(data);
            if (StageManager.Instance != null) StageManager.Instance.CollectSaveData(data);
            if (GachaManager.Instance != null) GachaManager.Instance.CollectSaveData(data);

            data.LastExitTimeTicks = DateTime.UtcNow.Ticks;

            // DataManager 저장 대기
            await DataManager.Instance.SaveGameData(data);
            
            // DataManager.Instance.OnResetButtonClicked();
        }
        
        /*
        public async void SaveGameOnExit()
        {
            GameSaveData data = new GameSaveData();
        
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.CollectSaveData(data);
            }
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.CollectSaveData(data);
            }
            if (EquipmentCollectionManager.Instance != null)
            {
                EquipmentCollectionManager.Instance.CollectSaveData(data);
            }
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.CollectSaveData(data);
            }
            if (SlotManager.Instance != null)
            {
                SlotManager.Instance.CollectSaveData(data);
            }
            if (StageManager.Instance != null)
            {
                StageManager.Instance.CollectSaveData(data);
            }

            data.LastExitTimeTicks = DateTime.UtcNow.Ticks;

            await DataManager.Instance.SaveGameData(data);

            // DataManager.Instance.OnResetButtonClicked(); // Data 삭제후 테스트할거면 해제하면 됌
        }
        */
        
        /// <summary>
        /// 방치 보상을 계산하고 지급합니다.
        /// </summary>
        private void CalculateIdleReward(GameSaveData data)
        {
            DataManager.Instance.CalculateIdleReward();
        }

        private void OnApplicationQuit()
        {
            _ = SaveGameOnExitAsync();
        }
    }
}