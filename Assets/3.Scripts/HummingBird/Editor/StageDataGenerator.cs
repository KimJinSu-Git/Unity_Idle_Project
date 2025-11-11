using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Bird.Idle.Data;

namespace Bird.Editor
{
    /// <summary>
    /// CSV 파일을 읽어 StageData Scriptable Object들을 생성하거나 업데이트하는 에디터 툴
    /// </summary>
    public static class StageDataGenerator
    {
        private const string CSV_PATH = "Assets/3.Scripts/HummingBird/Data/CSV/Stage/StageData.csv";
        private const string SO_FOLDER_PATH = "Assets/3.Scripts/HummingBird/Data/ScriptableObject/Stage/";

        [MenuItem("Tools/Bird/Generate Stage Data (CSV)")]
        public static void GenerateStageData()
        {
            if (!File.Exists(CSV_PATH))
            {
                Debug.LogError($"[DataGenerator] CSV 파일을 찾을 수 없습니다: {CSV_PATH}");
                return;
            }

            // SO 저장 폴더가 없으면 생성
            if (!Directory.Exists(SO_FOLDER_PATH))
            {
                Directory.CreateDirectory(SO_FOLDER_PATH);
            }

            string[] lines = File.ReadAllLines(CSV_PATH);
            int generatedCount = 0;

            for (int i = 1; i < lines.Length; i++) 
            {
                string[] values = lines[i].Split(',');
                
                if (values.Length < 8) continue; 

                // 데이터 파싱
                if (TryParseStageData(values, out StageData newStageData))
                {
                    // 기존 SO 파일 로드 시도
                    string soFilePath = SO_FOLDER_PATH + $"StageData_{newStageData.StageID}.asset";
                    StageData existingAsset = AssetDatabase.LoadAssetAtPath<StageData>(soFilePath);

                    if (existingAsset == null)
                    {
                        AssetDatabase.CreateAsset(newStageData, soFilePath);
                    }
                    else
                    {
                        EditorUtility.CopySerialized(newStageData, existingAsset);
                        EditorUtility.SetDirty(existingAsset);
                        Object.DestroyImmediate(newStageData);
                    }
                    generatedCount++;
                }
            }

            // 에셋 저장 및 업데이트
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[DataGenerator] StageData.asset 업데이트 완료. 총 {generatedCount}개 스테이지 SO 생성/업데이트.");
        }

        /// <summary>
        /// CSV 배열에서 StageData 객체로 데이터를 파싱
        /// </summary>
        private static bool TryParseStageData(string[] values, out StageData stageData)
        {
            stageData = ScriptableObject.CreateInstance<StageData>();

            // StageID
            if (!int.TryParse(values[0], out stageData.StageID)) return false;
            
            // StageName
            stageData.StageName = values[1].Trim();
            
            // MonsterIDs - 세미콜론(;)으로 구분
            stageData.MonsterIDs = new List<int>();
            string[] monsterIdStrings = values[2].Split(';');
            foreach (string idStr in monsterIdStrings)
            {
                if (int.TryParse(idStr.Trim(), out int monsterID) && monsterID != -1)
                {
                    stageData.MonsterIDs.Add(monsterID);
                }
            }

            // MonsterKillCountRequired
            if (!int.TryParse(values[3], out stageData.MonsterKillCountRequired)) return false;

            // IsBossStage
            if (!bool.TryParse(values[4].Trim().ToUpper(), out stageData.IsBossStage)) return false;

            // BossMonsterID
            if (!int.TryParse(values[5], out stageData.BossMonsterID)) return false;

            // GoldRewardMultiplier
            if (!float.TryParse(values[6], out stageData.GoldRewardMultiplier)) return false;

            // ExpRewardMultiplier
            if (!float.TryParse(values[7], out stageData.ExpRewardMultiplier)) return false;

            return true;
        }
    }
}