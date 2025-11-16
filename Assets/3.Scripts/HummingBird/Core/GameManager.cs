using System;
using System.Collections.Generic;
using UnityEngine;
using Bird.Idle.Core;
using Bird.Idle.Data;
using Bird.Idle.Gameplay;
using Bird.Idle.UI;

namespace Bird.Idle.Core
{
    /// <summary>
    /// 게임의 전역 상태 및 흐름을 제어하는 싱글톤 클래스
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private BattleManager battleManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            battleManager = BattleManager.Instance;
            
            StartGameFlow();
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
            
            CalculateIdleReward(loadedData);
            
            // SetBattleState(false);
        }
        
        /// <summary>
        /// 로드된 데이터를 각 관리자에게 전달하고 초기화
        /// </summary>
        private void ApplyLoadedDataToManagers(GameSaveData data)
        {
            CurrencyManager.Instance.InitializeGold(data.GoldAmount);
            CharacterManager.Instance.Initialize(data);
            StageManager.Instance.Initialize(data);
            
            Dictionary<int, EquipmentData> allEquipmentMap = EquipmentCollectionManager.Instance?.AllEquipmentSO;
            
            EquipmentCollectionManager.Instance.Initialize(data.CollectionEntries);
            InventoryManager.Instance.Initialize(data.EquippedItems, allEquipmentMap);
            SlotManager.Instance.Initialize(data.SlotLevels);
        
            Debug.Log("[GameManager] 로드된 데이터로 모든 관리자 초기화 완료.");
        }
        
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

            DataManager.Instance.OnResetButtonClicked(); // Data 삭제후 테스트할거면 해제하면 됌
        }
        
        /// <summary>
        /// 방치 보상을 계산하고 지급합니다.
        /// </summary>
        private void CalculateIdleReward(GameSaveData data)
        {
            DataManager.Instance.CalculateIdleReward();
        }

        private void OnApplicationQuit()
        {
            SaveGameOnExit();
        }
    }
}