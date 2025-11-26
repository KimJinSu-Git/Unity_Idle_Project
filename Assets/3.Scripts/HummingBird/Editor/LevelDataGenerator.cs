using UnityEngine;
using UnityEditor;
using System.IO;
using Bird.Idle.Data;
using System.Collections.Generic;

namespace Bird.Editor
{
    /// <summary>
    /// CSV 파일을 읽어 LevelUpCostData Scriptable Object를 생성하거나 업데이트하는 에디터 툴
    /// </summary>
    public static class LevelDataGenerator
    {
        // 파일 경로
        private const string CSV_PATH = "Assets/3.Scripts/HummingBird/Data/CSV/Player/LevelUpCostData.csv";
        private const string SO_PATH = "Assets/3.Scripts/HummingBird/Data/ScriptableObject/Player/LevelUpCostData.asset";

        [MenuItem("Tools/Bird/Generate Level Data (CSV)")]
        public static void GenerateLevelData()
        {
            if (!File.Exists(CSV_PATH))
            {
                Debug.LogError($"[DataGenerator] CSV 파일을 찾을 수 없습니다: {CSV_PATH}");
                return;
            }

            // LevelData SO 에셋을 로드하거나 새로 생성
            LevelUpCostData dataAsset = AssetDatabase.LoadAssetAtPath<LevelUpCostData>(SO_PATH);
            if (dataAsset == null)
            {
                dataAsset = ScriptableObject.CreateInstance<LevelUpCostData>();
                AssetDatabase.CreateAsset(dataAsset, SO_PATH);
                Debug.LogWarning("[DataGenerator] LevelData.asset 파일이 없어 새로 생성했습니다.");
            }
            
            dataAsset.LevelTable.Clear();

            string[] lines = File.ReadAllLines(CSV_PATH);
            
            for (int i = 1; i < lines.Length; i++) 
            {
                string[] values = lines[i].Split(',');
                
                if (values.Length < 2) continue; 

                if (int.TryParse(values[0], out int level) && long.TryParse(values[1], out long requiredExp))
                {
                    dataAsset.LevelTable.Add(new LevelUpCostData.LevelEntry
                    {
                        Level = level,
                        RequiredEXP = requiredExp,
                    });
                }
            }

            // 에셋 저장 및 업데이트
            EditorUtility.SetDirty(dataAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[DataGenerator] LevelUpCostData.asset 업데이트 완료. 총 {dataAsset.LevelTable.Count}개 항목.");
        }
    }
}