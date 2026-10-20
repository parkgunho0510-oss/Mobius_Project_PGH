public static class QuestObjectiveMatcher
{
    public static bool IsMatch(QuestObjectiveData objectiveData, ObjectiveRunTimeState objectiveState, QuestObjectiveEvent objectiveEvent)
    {
        if (objectiveData == null)
            return false;

        if (objectiveState == null)
            return false;

        if (objectiveState.IsCompleted)
            return false;

        if (objectiveData.Type == QuestObjectiveType.BRING_ITEM)
            return false;

        if (objectiveData.Type != objectiveEvent.Type)
            return false;

        if (objectiveData.TargetID != objectiveEvent.TargetID)
            return false;

        return true;
    }
}