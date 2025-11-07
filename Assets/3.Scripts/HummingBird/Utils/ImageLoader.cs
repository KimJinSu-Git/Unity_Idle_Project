using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

namespace Bird.Idle.Utils
{
    /// <summary>
    /// Addressables Key를 받아 Image 컴포넌트에 Sprite를 비동기로 로드하는 유틸리티
    /// </summary>
    public class ImageLoader : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        private AsyncOperationHandle<Sprite> currentHandle;

        private void Awake()
        {
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }
        }

        public async Task LoadSprite(string address)
        {
            Debug.Log("몇 번 호출했나요 ?");
            if (string.IsNullOrEmpty(address))
            {
                ClearSprite();
                return;
            }
            
            if (currentHandle.IsValid())
            {
                Addressables.Release(currentHandle);
            }

            currentHandle = Addressables.LoadAssetAsync<Sprite>(address);
            
            await currentHandle.Task;

            if (currentHandle.Status == AsyncOperationStatus.Succeeded)
            {
                if (targetImage != null) 
                {
                    targetImage.sprite = currentHandle.Result;
                    targetImage.color = new Color(1, 1, 1, 1);
                } 
                else
                {
                    Addressables.Release(currentHandle); 
                }
            }
            else
            {
                Debug.LogError($"[ImageLoader] Sprite 로드 실패: {currentHandle.OperationException}");
                ClearSprite();
            }
        }

        public void ClearSprite()
        {
            if (currentHandle.IsValid())
            {
                Addressables.Release(currentHandle);
            }
            targetImage.sprite = null;
            targetImage.color = new Color(1, 1, 1, 0);
        }
        
        private void OnDestroy()
        {
            ClearSprite();
        }
    }
}