using UnityEngine;
using System.Collections.Generic;
using Bird.Idle.Gameplay;

namespace Bird.Idle.UI
{
    public class MapUIManager : MonoBehaviour
    {
        public static MapUIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject mapPanelRoot;
        [SerializeField] private StageListPopup stageListPopup;
        [SerializeField] private StageConfirmationPopup confirmPopup;

        [Header("Chapters")]
        [SerializeField] private List<ChapterButton> chapterButtons; 

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            
            CloseAll();
        }

        public void OpenMap()
        {
            mapPanelRoot.SetActive(true);
            stageListPopup.Close();
            confirmPopup.gameObject.SetActive(false);

            foreach (var btn in chapterButtons)
            {
                btn.UpdateChapterState();
            }
        }

        public void CloseMap()
        {
            mapPanelRoot.SetActive(false);
        }

        private void CloseAll()
        {
            mapPanelRoot.SetActive(false);
            stageListPopup.gameObject.SetActive(false);
            confirmPopup.gameObject.SetActive(false);
        }

        public void OpenStageList(int chapterID, string chapterName)
        {
            stageListPopup.Show(chapterID, chapterName);
        }

        public void ShowMoveConfirmation(int stageID, string stageName)
        {
            confirmPopup.Show(stageID, stageName, () =>
            {
                MoveToStage(stageID);
            });
        }

        private void MoveToStage(int stageID)
        {
            StageManager.Instance.RequestStageChange(stageID);
            
            CloseAll();
        }
    }
}