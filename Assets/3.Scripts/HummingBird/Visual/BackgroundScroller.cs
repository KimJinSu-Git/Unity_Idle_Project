using System;
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
        [SerializeField] private AssetReferenceT<Texture2D> mountainBackgroundRef;
        [SerializeField] private AssetReferenceT<Texture2D> desertBackgroundRef;
        [SerializeField] private AssetReferenceT<Texture2D> graveyardBackgroundRef;
        [SerializeField] private AssetReferenceT<Texture2D> snowBackgroundRef;
        
        [SerializeField] private MeshRenderer backgroundRenderer; 

        private static readonly int MainTexOffset = Shader.PropertyToID("_MainTex");
        
        private AsyncOperationHandle<Texture2D> currentBackgroundHandle;
        
        private AssetReferenceT<Texture2D> currentlyLoadedRef;
        
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
            if (battleManager != null)
            {
                battleManager.OnBattleStateChanged += SetMovementState; 
            }
        }

        private void Start()
        {
            backgroundRenderer = GetComponent<MeshRenderer>();
        }

        private void Update()
        {
            if (!GameManager.Instance.IsBattleActive) return;
            
            if (isMoving)
            {
                currentOffset += Time.deltaTime * scrollSpeed;
                
                if (backgroundRenderer != null && backgroundRenderer.material != null)
                {
                    backgroundRenderer.material.SetTextureOffset(MainTexOffset, new Vector2(currentOffset, 0));
                }
            }
        }

        public void SetMovementState(bool isBattleActive)
        {
            isMoving = !isBattleActive;
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
            currentOffset = 0f;
            if (backgroundRenderer != null && backgroundRenderer.material != null)
            {
                backgroundRenderer.material.SetTextureOffset(MainTexOffset, Vector2.zero);
            }
            
            int mapIndex = (newStageID - 1) / 100;
            AssetReferenceT<Texture2D> nextBackgroundRef = mountainBackgroundRef;
            
            switch (mapIndex)
            {
                case 1: nextBackgroundRef = desertBackgroundRef; break;
                case 2: nextBackgroundRef = graveyardBackgroundRef; break;
                case 3: nextBackgroundRef = snowBackgroundRef; break;
                default: nextBackgroundRef = mountainBackgroundRef; break;
            }
            
            LoadNewBackground(nextBackgroundRef);
        }
        
        private async void LoadNewBackground(AssetReferenceT<Texture2D> backgroundRef)
        {
            if (backgroundRef == null || !backgroundRef.IsValid()) return;
            
            if (currentlyLoadedRef == backgroundRef)
            {
                Debug.Log($"[Scroller] 배경 {backgroundRef.AssetGUID}는 이미 로드되어 있습니다. 스킵.");
                return;
            }
            
            if (currentBackgroundHandle.IsValid())
            {
                Addressables.Release(currentBackgroundHandle);
                currentlyLoadedRef = null;
            }
            
            currentBackgroundHandle = backgroundRef.LoadAssetAsync();
            currentlyLoadedRef = backgroundRef;
            
            if (!currentBackgroundHandle.IsValid())
            {
                Debug.LogError("[Scroller] 로딩 핸들이 유효하지 않습니다.");
                return;
            }
            
            await currentBackgroundHandle.Task;

            if (currentBackgroundHandle.Status == AsyncOperationStatus.Succeeded)
            {
                backgroundRenderer.material.mainTexture = currentBackgroundHandle.Result;
                Debug.Log($"[Scroller] 배경 로드 성공: {backgroundRef.AssetGUID}");
            }
            else
            {
                Debug.LogError($"[Scroller] 배경 로드 실패: {currentBackgroundHandle.OperationException}");
                currentlyLoadedRef = null;
            }
        }
        
        private void OnDestroy()
        {
            if (StageManager.Instance != null)
            {
                StageManager.Instance.OnStageChanged -= HandleStageTransition;
            }
            
            if (battleManager != null)
            {
                battleManager.OnBattleStateChanged -= SetMovementState; 
            }
            
            if (currentBackgroundHandle.IsValid())
            {
                Addressables.Release(currentBackgroundHandle);
            }
        }
    }
}