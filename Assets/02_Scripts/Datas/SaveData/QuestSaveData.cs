using System;
using System.Collections.Generic;

[Serializable]
public class QuestSaveData
{
    public string QuestID;
    public QuestStatusType QuestStatusType;

    public int CompletedCount;
    public string LastCompletedAt;

    public List<QuestObjectiveSaveData> ObjectiveSaveDatas = new();
}