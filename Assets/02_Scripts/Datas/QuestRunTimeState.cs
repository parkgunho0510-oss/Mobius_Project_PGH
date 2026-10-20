using System.Collections.Generic;
[System.Serializable]
public class QuestRunTimeState
{
    public QuestData Data;
    public QuestStatusType Status;

    public int CompletedCount;
    public string LastCompletedAt;

    public List<ObjectiveRunTimeState> ObjectiveRunTimeStateList = new();
    public void ResetProgress()
    {
        foreach (ObjectiveRunTimeState state in ObjectiveRunTimeStateList)
        {
            state.CurrentAmount = 0;
            state.IsCompleted = false;
        }
    }
}
