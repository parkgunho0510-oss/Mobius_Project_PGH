public static class QuestStateMachine
{
    public static bool CanAccept(QuestRunTimeState state)
    {
        return state.Status == QuestStatusType.NOTSTARTED;
    }

    public static bool CanUpdateProgress(QuestRunTimeState state)
    {
        return state.Status == QuestStatusType.INPROGRESS;
    }

    public static bool CanComplete(QuestRunTimeState state)
    {
        return state.Status == QuestStatusType.CANCOMPLETE;
    }

    public static bool CanGiveUp(QuestRunTimeState state)
    {
        //TODO: Add canComplete
        return state.Status == QuestStatusType.INPROGRESS || state.Status == QuestStatusType.CANCOMPLETE;
    }

    public static bool TryAccept(QuestRunTimeState state)
    {
        if (!CanAccept(state))
            return false;

        state.Status = QuestStatusType.INPROGRESS;
        return true;
    }

    public static bool TrySetCanComplete(QuestRunTimeState state)
    {
        if (state.Status != QuestStatusType.INPROGRESS)
            return false;

        state.Status = QuestStatusType.CANCOMPLETE;
        return true;
    }

    public static bool TryComplete(QuestRunTimeState state)
    {
        if (!CanComplete(state))
            return false;

        state.Status = QuestStatusType.COMPLETED;
        return true;
    }

    public static bool TryGiveUp(QuestRunTimeState state)
    {
        if (!CanGiveUp(state))
            return false;

        state.Status = QuestStatusType.NOTSTARTED;
        state.ResetProgress();
        return true;
    }

    public static bool IsVisibleInQuestLog(QuestStatusType status)
    {
        return status == QuestStatusType.INPROGRESS ||
               status == QuestStatusType.CANCOMPLETE;
    }

    public static bool IsVisibleInQuestLog(QuestRunTimeState state)
    {
        return IsVisibleInQuestLog(state.Status);
    }

    public static bool IsVisibleInQuickSlot(QuestStatusType status)
    {
        return status == QuestStatusType.INPROGRESS ||
               status == QuestStatusType.CANCOMPLETE;
    }
    public static bool IsVisibleInQuickSlot(QuestRunTimeState state)
    {
        return IsVisibleInQuickSlot(state.Status);
    }
    public static bool IsActiveQuest(QuestRunTimeState state)
    {
        if (state == null)
            return false;

        return IsActiveQuest(state.Status);
    }

    public static bool IsActiveQuest(QuestStatusType status)
    {
        return status == QuestStatusType.INPROGRESS ||
               status == QuestStatusType.CANCOMPLETE;
    }

    public static bool TryResetToNotStarted(QuestRunTimeState state)
    {
        if (state == null)
            return false;

        if (state.Status != QuestStatusType.COMPLETED)
            return false;

        state.Status = QuestStatusType.NOTSTARTED;
        state.ResetProgress();
        return true;
    }
}