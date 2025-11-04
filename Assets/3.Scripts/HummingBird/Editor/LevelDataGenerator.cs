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

            // CSV 파일 읽기 및 파싱
            string[] lines = File.ReadAllLines(CSV_PATH);
            // 첫 줄 건너뛰기(1부터 시작)
            for (int i = 1; i < lines.Length; i++) 
            {
                string[] values = lines[i].Split(',');
                if (values.Length < 4) continue;

                // 파싱 및 데이터 유효성 검사
                if (int.TryParse(values[0], out int level) && long.TryParse(values[1], out long requiredGold) &&
                    float.TryParse(values[2], out float attackIncrease) && float.TryParse(values[3], out float healthIncrease))
                {
                    // SO 데이터에 추가
                    dataAsset.LevelTable.Add(new LevelUpCostData.LevelEntry
                    {
                        Level = level,
                        RequiredGold = requiredGold,
                        AttackIncrease = attackIncrease,
                        HealthIncrease = healthIncrease
                    });
                }
            }

            // 에셋 저장 및 업데이트
            EditorUtility.SetDirty(dataAsset); // 변경사항 표시
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[DataGenerator] LevelUpCostData.asset 업데이트 완료. 총 {dataAsset.LevelTable.Count}개 항목.");
        }
    }
}