using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bird.Idle.Gameplay;
using System.Collections.Generic;
using Bird.Idle.Data;

namespace Bird.Idle.UI
{
    public class UI_GachaPanel : MonoBehaviour
    {
        [Header("Status UI")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Slider expSlider;
        [SerializeField] private TextMeshProUGUI expText;
        [SerializeField] private TextMeshProUGUI probabilityInfoText;

        [Header("Buttons")]
        [SerializeField] private Button summon1Button;
        [SerializeField] private Button summon10Button;
        
        [Header("Popups")]
        [SerializeField] private UI_GachaResultPopup resultPopup;

        private void Start()
        {
            summon1Button.onClick.AddListener(() => OnSummonClicked(1));
            summon10Button.onClick.AddListener(() => OnSummonClicked(10));

            if (GachaManager.Instance != null)
            {
                GachaManager.Instance.OnGachaExpChanged += UpdateLevelUI;
                GachaManager.Instance.OnGachaFinished += ShowResult;
                
                var status = GachaManager.Instance.GetCurrentStatus();
                UpdateLevelUI(status.level, status.curExp, status.maxExp);
            }
        }

        private void OnDestroy()
        {
            if (GachaManager.Instance != null)
            {
                GachaManager.Instance.OnGachaExpChanged -= UpdateLevelUI;
                GachaManager.Instance.OnGachaFinished -= ShowResult;
            }
        }

        private void OnSummonClicked(int count)
        {
            GachaManager.Instance.TrySummon(count);
        }

        private void UpdateLevelUI(int level, int currentExp, int maxExp)
        {
            levelText.text = $"Machine Lv.{level}";
            
            if (maxExp > 0)
            {
                float ratio = (float)currentExp / maxExp;
                expSlider.value = ratio;
                expText.text = $"{currentExp} / {maxExp}";
            }
            else
            {
                expSlider.value = 1f;
                expText.text = "MAX";
            }

            // TODO ::: 현재 레벨의 확률 정보 표기
        }

        private void ShowResult(List<EquipmentData> items)
        {
            if (resultPopup != null)
            {
                resultPopup.Show(items);
            }
        }
    }
}