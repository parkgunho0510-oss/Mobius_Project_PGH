using UnityEngine;

[System.Serializable]
public class LevelData : IDataEntity
{
    // CSV Header: ID (Ex: LVL_001)
    [SerializeField] private string id;
    public string ID => id;

    public int Level;
    public int NextExp;
    public int TotalExp;

   
    public LevelData() { }

    public void LinkData(DataManager manager)
    {

    }
}
