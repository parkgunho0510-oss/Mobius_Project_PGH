using UnityEngine;

[System.Serializable]
public class StageMonsterData : IDataEntity
{
    // CSV Header : ID (Ex: STAGEM_001)
    [SerializeField] private string id;
    public string ID => id;

    // CSV Header : StageId (Link to StageData - ID)
    // ex: STAGE_001 이라는 값과 같은 행들이 이 스테이지의 스폰 목록이 됨
    public string StageID;

    // CSV Header: MonsterName
    public string MonsterName;

    // CSV Header: MonsterPrefab 
    public GameObject MonsterPrefab;

    // CSV Header: MinSpawnCount, MaxSpawnCount
    public int MinSpawnCount;
    public int MaxSpawnCount;

    public StageMonsterData() { }

    public void LinkData(DataManager manager)
    {
       
    }
}