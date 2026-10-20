using System;
using UnityEngine;

[System.Serializable]
public class QuestRewardData : IDataEntity
{
    public string Id;
    public string ID => Id;
    public string QuestID;
    public QuestRewardType Type;
    public string RewardID;
    public int Amount;

    [TextArea]
    public string Description;

    //[Header("Test")]
    //[SerializeField] public QuestData QuestData;
    public void LinkData(DataManager dataManager)
    {

    }
}
