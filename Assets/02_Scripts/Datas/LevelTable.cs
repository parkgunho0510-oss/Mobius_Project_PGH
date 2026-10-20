using UnityEngine;
using System.Collections.Generic;
public class LevelTable
{
    readonly List<LevelData> _levels = new();
    public int MaxLevel => _levels.Count;
    public void LoadFromCsv()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("Excel/LevelTable");

        if(csvFile == null)
        {
            Debug.Log("LevelTable is NULL");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        for(int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            if(values.Length < 3)
            {
                Debug.Log($"{line} is wrong");
                continue;
            }

            int level = int.Parse(values[0]);
            int nextExp = int.Parse(values[1]);
            int totalExp = int.Parse(values[2]);

            //_levels.Add(new LevelData(level, nextExp, totalExp));
        }
    } 

    public LevelData GetLevelDataByLevel(int level)
    {
        if (level < 1 || level > _levels.Count) return null;
        else return _levels[level - 1];
    }
    public int GetTotalExpByLevel(int currentLevel)
    {
        if (currentLevel < 1 || currentLevel > _levels.Count) return -1;
        else return _levels[currentLevel - 1].TotalExp;
    }
    public int GetRequireExp(int currentLevel)
    {
        if (currentLevel < 1 || currentLevel + 1 > _levels.Count) return -1;
        else return _levels[currentLevel - 1].NextExp;
    }
}
