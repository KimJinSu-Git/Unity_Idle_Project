using System.Collections;
using Bird.Idle.Core;
using UnityEngine;
using Bird.Idle.Gameplay;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using StageManager = Bird.Idle.Gameplay.StageManager;

namespace Bird.Idle.Visual
{
    /// <summary>
    /// 플레이어의 공격 주기에 맞춰 배경을 이동시켜 진행감을 표현하는 스크롤러
    /// </summary>
    public class BackgroundScroller : MonoBehaviour
    {
        [Header("Scroll Settings")]
        [SerializeField] private float scrollSpeed = 0.5f; // 배경 이동 속도
        [SerializeField] private float stopDuration = 0.1f; // 공격 시 멈추는 시간
        
        [Header("Data References")]
        [SerializeField] private AssetReferenceT<Texture2D> desertBackgroundRef;
        
        [SerializeField] private Renderer backgroundRenderer; 

        private BattleManager battleManager;
        private bool isMoving = true;
        private float currentOffset = 0f;
        
        private void Awake()
        {
            battleManager = BattleManager.Instance;
            
            if (StageManager.Instance != null)
            {
                StageManager.Instance.OnStageChanged += HandleStageTransition;
            }
            // battleManager.OnBattleStateChanged += SetMovementState;
        }

        private void Update()
        {
            if (!GameManager.Instance.IsBattleActive) return;

            if (isMoving)
            {
                currentOffset += Time.deltaTime * scrollSpeed;
                backgroundRenderer.material.mainTextureOffset = new Vector2(-currentOffset, 0);
            }
        }

        public void SetMovementState(bool isBattleActive)
        {
            // isBattleActive == false (몬스터 전투 중)일 때 멈추고 싶다면
            // isMoving = !isBattleActive; 
    
            // 현재는 공격 애니메이션 시 멈추는 로직이므로, IsBattleActive 상태를 따릅니다.
            isMoving = !isBattleActive; // 전투 중(Active)일 때 멈추고, 전투 아닐 때(이동) 움직입니다.
        }

        private IEnumerator StartMoveAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            isMoving = true;
        }
        
        /// <summary>
        /// 스테이지 클리어/진입 시 호출되어 배경을 초기화하고 페이드 인/아웃을 처리
        /// </summary>
        public void HandleStageTransition()
        {
            // TODO: StageManager가 Stage 클리어 시 호출
            // 배경 페이드 아웃/인 처리
            // 배경 위치 초기화 (textureOffset = Vector2.zero)
            // 맵 변경 로직
        }
        
        /// <summary>
        /// 스테이지 클리어/진입 시 호출되어 배경을 초기화하고 페이드 인/아웃을 처리
        /// </summary>
        public void HandleStageTransition(int newStageID)
        {
            // TODO: 페이드 인/아웃 코루틴 시작

            if (newStageID > 100)
            {
                LoadNewBackground(desertBackgroundRef);
            }
            else
            {
                // 배경 스크롤 위치 초기화 (배경 유지)
                backgroundRenderer.material.mainTextureOffset = Vector2.zero;
            }
        }
        
        private async void LoadNewBackground(AssetReferenceT<Texture2D> backgroundRef)
        {
            var handle = backgroundRef.LoadAssetAsync();
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                backgroundRenderer.material.mainTexture = handle.Result;
            }
            else
            {
                Debug.LogError($"배경 로드 실패: {handle.OperationException}");
            }
        }
        
        private void OnDestroy()
        {
            if (StageManager.Instance != null)
            {
                StageManager.Instance.OnStageChanged -= HandleStageTransition;
            }
        }
    }
}