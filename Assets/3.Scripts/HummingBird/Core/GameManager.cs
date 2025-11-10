using System;
using UnityEngine;
using Bird.Idle.Core;
using Bird.Idle.Data;
using Bird.Idle.Gameplay;

namespace Bird.Idle.Core
{
    /// <summary>
    /// 게임의 전역 상태 및 흐름을 제어하는 싱글톤 클래스
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private bool isBattleActive = false;
        public bool IsBattleActive => isBattleActive;

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
            
            isBattleActive = false;
            
            battleManager = BattleManager.Instance;
            
            StartGameFlow();
        }

        /// <summary>
        /// 게임 시작 시 초기 흐름을 관리
        /// </summary>
        private async void StartGameFlow()
        {
            GameSaveData loadedData = await DataManager.Instance.LoadGameData();
            
            ApplyLoadedDataToManagers(loadedData);
            
            CalculateIdleReward(loadedData);
            
            // SetBattleState(false);
        }
        
        /// <summary>
        /// 로드된 데이터를 각 관리자에게 전달하고 초기화
        /// </summary>
        private void ApplyLoadedDataToManagers(GameSaveData data)
        {
            // TODO: SlotManager
            CurrencyManager.Instance.InitializeGold(data.GoldAmount);
            CharacterManager.Instance.Initialize(data);
            EquipmentCollectionManager.Instance.Initialize(data.CollectionEntries);
        
            Debug.Log("[GameManager] 로드된 데이터로 모든 관리자 초기화 완료.");
        }
        
        public async void SaveGameOnExit()
        {
            GameSaveData data = new GameSaveData();
        
            if (CharacterManager.Instance != null)
            {
                Debug.Log("[CharacterManager] CollectSaveData 호출");
                CharacterManager.Instance.CollectSaveData(data);
            }
            if (CurrencyManager.Instance != null)
            {
                Debug.Log("[CurrencyManager] CollectSaveData 호출");
                CurrencyManager.Instance.CollectSaveData(data);
            }
            if (EquipmentCollectionManager.Instance != null)
            {
                Debug.Log("[EquipmentCollectionManager] CollectSaveData 호출");
                EquipmentCollectionManager.Instance.CollectSaveData(data);
            }
        
            data.LastExitTimeTicks = DateTime.UtcNow.Ticks;

            await DataManager.Instance.SaveGameData(data);
        }
        
        /// <summary>
        /// 방치 보상을 계산하고 지급합니다.
        /// </summary>
        private void CalculateIdleReward(GameSaveData data)
        {
            // DataManager의 로직을 여기에 가져오거나, DataManager의 기존 메서드를 활용
        
            // DataManager.Instance.CalculateIdleReward(data.LastExitTimeTicks); // DataManager 수정 필요
        
            // 현재는 DataManager의 기존 로직을 그대로 호출
            DataManager.Instance.CalculateIdleReward(); // DataManager 내부에서 lastExitTime 사용
        }

        /// <summary>
        /// 게임의 전투 활성화 상태를 변경
        /// </summary>
        public void SetBattleState(bool active)
        {
            isBattleActive = active;
            if (battleManager != null)
            {
                battleManager.SetBattleActive(active); 
            }
            Debug.Log($"[GameManager] 전투 상태 변경: {active}");
        }
        
        // TODO: 방치 보상 팝업 닫히면 호출할 메서드
        public void ResumeGameAfterAFK()
        {
            SetBattleState(true);
            Debug.Log("[GameManager] 방치 보상 완료, 전투 재개.");
        }

        private void OnApplicationQuit()
        {
            SaveGameOnExit();
        }
    }
}