public readonly struct QuestObjectiveEvent
{
    public readonly QuestObjectiveType Type;
    public readonly string TargetID;
    public readonly int Amount;

    public QuestObjectiveEvent(
        QuestObjectiveType type,
        string targetID,
        int amount = 1)
    {
        Type = type;
        TargetID = targetID;
        Amount = amount;
    }
}