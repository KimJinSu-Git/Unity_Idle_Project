using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Bird.Idle.UI
{
    public class StageListPopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform contentContainer;
        [SerializeField] private StageNode stageNodePrefab;
        [SerializeField] private Button closeButton;

        private List<StageNode> spawnedNodes = new List<StageNode>();

        private void Awake()
        {
            closeButton.onClick.AddListener(Close);
        }

        public void Show(int chapterID, string chapterName)
        {
            titleText.text = chapterName;
            
            foreach (var node in spawnedNodes)
            {
                if(node != null) Destroy(node.gameObject);
            }
            spawnedNodes.Clear();

            int startStageID = (chapterID - 1) * 10 + 1;
            int endStageID = startStageID + 9;

            for (int id = startStageID; id <= endStageID; id++)
            {
                StageNode newNode = Instantiate(stageNodePrefab, contentContainer);
                
                int localStageNumber = id - ((chapterID - 1) * 10);
                string displayName = $"{chapterID}-{localStageNumber}";
                
                newNode.Setup(id, displayName);
                spawnedNodes.Add(newNode);
            }

            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}