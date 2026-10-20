using UnityEngine;
[System.Serializable]
public class QuestObjectiveData : IDataEntity
{
    public string Id;
    public string ID => Id;
    public string QuestID;
    public QuestObjectiveType Type;
    public string TargetID;
    public int RequireAmount;
    
    [TextArea]
    public string Description;

    public void LinkData(DataManager dataManager)
    {

    }
}
