using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bird.Idle.Gameplay;
using System.Collections.Generic;
using System.Text;
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

            UpdateProbabilityText();
        }
        
        private void UpdateProbabilityText()
        {
            if (GachaManager.Instance == null) return;

            GachaLevelInfo info = GachaManager.Instance.GetCurrentLevelInfo();

            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine($"Common : {info.CommonProb} %");
            sb.AppendLine($"Rare : {info.RareProb} %");
            sb.AppendLine($"Epic : {info.EpicProb} %");
            sb.AppendLine($"Legendary : {info.LegendaryProb} %");

            probabilityInfoText.text = sb.ToString();
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