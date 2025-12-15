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
        
        [Header("Data References")]
        [SerializeField] private AssetReferenceT<Texture2D> mountainBackgroundRef;
        [SerializeField] private AssetReferenceT<Texture2D> desertBackgroundRef;
        [SerializeField] private AssetReferenceT<Texture2D> graveyardBackgroundRef;
        [SerializeField] private AssetReferenceT<Texture2D> snowBackgroundRef;
        
        [Header("Height Offsets")]
        [Tooltip("기본 위치보다 얼마나 위/아래로 움직일지 설정(이미지의 발판 위치랑 Player의 위치가 맞게 할려고 추가하였습니다)")]
        [SerializeField] private float mountainYOffset = 0f;
        [SerializeField] private float desertYOffset = 0.3f;
        [SerializeField] private float graveyardYOffset = 0f;
        [SerializeField] private float snowYOffset = 0.6f;
        
        [SerializeField] private MeshRenderer backgroundRenderer;
        [SerializeField] private PlayerController playerController;

        private static readonly int MainTexOffset = Shader.PropertyToID("_MainTex");
        
        private AsyncOperationHandle<Texture2D> currentBackgroundHandle;
        private AssetReferenceT<Texture2D> currentlyLoadedRef;
        
        private BattleManager battleManager;
        
        private float currentOffset = 0f;
        private float initialYPosition;
        
        private void Awake()
        {
            battleManager = BattleManager.Instance;
            
            initialYPosition = transform.position.y;
            
            if (StageManager.Instance != null)
            {
                StageManager.Instance.OnStageChanged += HandleStageTransition;
            }
        }

        private void Start()
        {
            backgroundRenderer = GetComponent<MeshRenderer>();
        }

        private void Update()
        {
            if (battleManager.PlayerBattleMode) return;
            
            if (playerController != null && playerController.GetAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == playerController.GetRunAnimHash)
            {
                currentOffset += Time.deltaTime * scrollSpeed;
                
                if (backgroundRenderer != null && backgroundRenderer.material != null)
                {
                    backgroundRenderer.material.SetTextureOffset(MainTexOffset, new Vector2(currentOffset, 0));
                }
            }
        }
        
        /// <summary>
        /// 스테이지 클리어/진입 시 호출되어 배경을 초기화하고 페이드 인/아웃을 처리
        /// </summary>
        public void HandleStageTransition(int newStageID)
        {
            currentOffset = 0f;
            if (backgroundRenderer != null && backgroundRenderer.material != null)
            {
                backgroundRenderer.material.SetTextureOffset(MainTexOffset, Vector2.zero);
            }
            
            int mapIndex = (newStageID - 1) / 10;
            AssetReferenceT<Texture2D> nextBackgroundRef = mountainBackgroundRef;
            float targetYOffset = mountainYOffset;
            
            switch (mapIndex)
            {
                case 1: 
                    nextBackgroundRef = desertBackgroundRef; 
                    targetYOffset = desertYOffset;
                    break;
                case 2: 
                    nextBackgroundRef = graveyardBackgroundRef; 
                    targetYOffset = graveyardYOffset;
                    break;
                case 3: 
                    nextBackgroundRef = snowBackgroundRef; 
                    targetYOffset = snowYOffset;
                    break;
                default: 
                    nextBackgroundRef = mountainBackgroundRef;
                    targetYOffset = mountainYOffset;
                    break;
            }
            
            LoadNewBackground(nextBackgroundRef, targetYOffset);
        }
        
        private async void LoadNewBackground(AssetReferenceT<Texture2D> backgroundRef, float yOffset)
        {
            if (backgroundRef == null) return;
            
            if (currentlyLoadedRef == backgroundRef)
            {
                Debug.Log($"[Scroller] 배경 {backgroundRef.AssetGUID}는 이미 로드되어 있습니다. 스킵.");
                ApplyHeightOffset(yOffset);
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
                
                ApplyHeightOffset(yOffset);
                
                Debug.Log($"[Scroller] 배경 로드 성공");
            }
            else
            {
                Debug.LogError($"[Scroller] 배경 로드 실패");
                currentlyLoadedRef = null;
            }
        }
        
        private void ApplyHeightOffset(float yOffset)
        {
            Vector3 newPos = transform.position;
            newPos.y = initialYPosition + yOffset;
            transform.position = newPos;
        }
        
        private void OnDestroy()
        {
            if (StageManager.Instance != null)
            {
                StageManager.Instance.OnStageChanged -= HandleStageTransition;
            }
            
            if (currentBackgroundHandle.IsValid())
            {
                Addressables.Release(currentBackgroundHandle);
            }
        }
    }
}