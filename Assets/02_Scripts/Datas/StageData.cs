using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Objects/StageData")]
public class StageData : ScriptableObject, IDataEntity
{
    // [IDataEntity] Excel Header: ID (EX: STAGE_001)
    [SerializeField] private string id;
    public string ID => id;

    // Excel Header: StageType (Enum Matching)
    public StageType StageType;

    // Excel Header: StageDescription
    public string StageDescription;

    // DataManager would fill Automatically MonsterList at runtime 
    [HideInInspector] public List<StageMonsterData> MonsterList = new();

    public void LinkData(DataManager manager)
    {
        MonsterList.Clear();
        // 전체 StageMonsterTable을 돌며 내 ID와 일치하는 것들만 수집
        foreach (var smData in manager.stageMonster.Table.Values)
        {
            if (smData.StageID == this.ID)
                MonsterList.Add(smData);
        }
    }
}
