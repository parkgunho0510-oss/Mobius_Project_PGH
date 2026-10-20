using System;
public static class QuestEvents
{
    public static event Action<QuestData> QuestAcceptedEvent;
    public static void RaiseQuestAccepted(QuestData data)
    {
        QuestAcceptedEvent?.Invoke(data);
    }
    public static event Action<QuestData> QuestCanCompleteEvent;
    public static void RaiseQuestCanComplete(QuestData data)
    {
        QuestCanCompleteEvent?.Invoke(data);
    }
    public static event Action<QuestData> QuestCompletedEvent;
    public static void RaiseQuestCompleted(QuestData data)
    {
        QuestCompletedEvent?.Invoke(data);
    }
    public static event Action<QuestData> QuestGiveUpEvent;
    public static void RaiseQuestGiveUp(QuestData data)
    {
        QuestGiveUpEvent?.Invoke(data);
    }
    public static event Action<QuestData> QuestUnLockEvent;
    public static void RaiseQuestUnLock(QuestData data)
    {
        QuestUnLockEvent?.Invoke(data);
    }

    public static event Action<MonsterDB> KillMonsterEvent;
    public static void RaiseKillMonster(MonsterDB data)
    {
        KillMonsterEvent?.Invoke(data);
    }
    public static event Action<ItemData, int> CollectItemEvent;
    public static void RaiseCollectItem(ItemData data, int amount)
    {
        CollectItemEvent?.Invoke(data, amount);
    }
    public static event Action<NpcData, ItemData, int> BringItemEvent;
    public static void RaiseBringItem(NpcData npcData, ItemData itemData, int amount)
    {
        BringItemEvent?.Invoke(npcData, itemData, amount);
    }
    public static event Action<NpcData> TalkToNpcEvent;
    public static void RaiseTalkToNpc(NpcData data)
    {
        TalkToNpcEvent?.Invoke(data);
    }

    public static event Action<string> OnAppectBtnClickEvent;
    public static void RaiseAcceptedBtnClick(string id)
    {
        OnAppectBtnClickEvent?.Invoke(id);
    }

    public static event Action<string> OnCompleteBtnClickEvent;
    public static void RaiseCompletedBtnClick(string id)
    {
        OnCompleteBtnClickEvent?.Invoke(id);
    }
    public static event Action<string> OnGiveUpBtnClickEvent;
    public static void RaiseGiveUpBtnClick(string id)
    {
        OnGiveUpBtnClickEvent?.Invoke(id);
    }
    public static event Action<string, float> ReputationUpdatedEvent;
    public static void RaiseReputationUpdated(string id, float value)
    {
        ReputationUpdatedEvent?.Invoke(id, value);
    }

    public static event Action QuestLogOpenedEvent;
    public static void RaiseQuestLogOpened()
    {
        QuestLogOpenedEvent?.Invoke();
    }
    public static event Action QuestLogClosedEvent;
    public static void RaiseQuestLogClosed()
    {
        QuestLogClosedEvent?.Invoke();
    }

    public static event Action<QuestData, QuestStatusType> QuestSaveDataLoadedEvent;
    
    public static void RaiseQuestSaveDataLoaded(QuestData data, QuestStatusType type)
    {
        QuestSaveDataLoadedEvent?.Invoke(data, type);
    }
}