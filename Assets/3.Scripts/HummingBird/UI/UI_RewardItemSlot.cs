using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bird.Idle.Data;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Bird.Idle.UI
{
    public class UI_RewardItemSlot : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image gradeBackgroundImage; 
        [SerializeField] private TextMeshProUGUI countText;

        private AsyncOperationHandle<Sprite> iconHandle;

        public void SetData(EquipmentData data, int count)
        {
            if (data == null) return;

            LoadIcon(data.iconAddress);

            countText.text = count > 1 ? $"x{count}" : "";

            if (gradeBackgroundImage != null)
            {
                gradeBackgroundImage.color = GetColorByGrade(data.grade);
            }
        }

        private void LoadIcon(string address)
        {
            if (iconHandle.IsValid())
            {
                Addressables.Release(iconHandle);
            }

            if (!string.IsNullOrEmpty(address))
            {
                iconHandle = Addressables.LoadAssetAsync<Sprite>(address);
                iconHandle.Completed += (handle) =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        if (iconImage != null)
                        {
                            iconImage.sprite = handle.Result;
                            iconImage.enabled = true;
                        }
                    }
                    else
                    {
                        Debug.LogError($"[ItemSlot] 아이콘 로드 실패: {address}");
                    }
                };
            }
            else
            {
                if (iconImage != null) iconImage.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (iconHandle.IsValid())
            {
                Addressables.Release(iconHandle);
            }
        }

        private Color GetColorByGrade(EquipmentGrade grade)
        {
            switch (grade)
            {
                case EquipmentGrade.Common: return Color.gray;
                case EquipmentGrade.Rare: return Color.cyan;
                case EquipmentGrade.Epic: return Color.magenta;
                case EquipmentGrade.Legendary: return Color.yellow;
                default: return Color.white;
            }
        }
    }
}