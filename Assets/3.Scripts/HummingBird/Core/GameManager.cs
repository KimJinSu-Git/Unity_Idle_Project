using UnityEngine;
using Bird.Idle.Core;
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
        private void StartGameFlow()
        {
            // TODO: DataManager.LoadGameData()가 완료될 때까지 기다리는 로직 필요
            
            SetBattleState(false);
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
    }
}