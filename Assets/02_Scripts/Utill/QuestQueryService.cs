using System.Collections.Generic;
using System.Linq;

public class QuestQueryService
{
    readonly QuestRegistry _questRegistry;
    readonly IReadOnlyDictionary<string, QuestRunTimeState> _runtimeStateTable;
    readonly PrerequisiteGraph _prerequisiteGraph;

    public QuestQueryService(QuestRegistry questRegistry,
        IReadOnlyDictionary<string, QuestRunTimeState> runtimeStateTable,
        PrerequisiteGraph prerequisiteGraph)
    {
        _questRegistry = questRegistry;
        _runtimeStateTable = runtimeStateTable;
        _prerequisiteGraph = prerequisiteGraph;
    }

    public List<QuestData> GetQuestsForNpc(string npcId)
    {
        IReadOnlyList<QuestData> candidates = _questRegistry.GetQuestsByNpcId(npcId);
        return candidates
            .Where(IsVisibleForNpc)
            .OrderBy(GetNpcQuestPriority)
            .ToList();
    }
    public List<QuestData> GetQuestLogList(QuestLogFilterType filter)
    {
        return _runtimeStateTable.Values
            .Where(state => IsMatchedLogFilter(state, filter))
            .OrderBy(state => GetQuestLogPriority(state))
            .Select(state => state.Data)
            .ToList();
    }
    bool IsVisibleForNpc(QuestData questData)
    {
        bool unlocked = _prerequisiteGraph.IsUnlocked(questData.Id, _runtimeStateTable);

        if (!unlocked)
            return false;

        if (!_runtimeStateTable.TryGetValue(questData.Id, out QuestRunTimeState state))
        {
            // 아직 수락하지 않은 퀘스트
            return true;
        }

        if (state.Status == QuestStatusType.COMPLETED)
        {
            // 완료된 퀘스트는 NPC 목록에서 숨김
            return CanShowCompletedQuestAgain(questData);
        }

        return state.Status == QuestStatusType.NOTSTARTED ||
               state.Status == QuestStatusType.INPROGRESS ||
               state.Status == QuestStatusType.CANCOMPLETE;
    }
    int GetNpcQuestPriority(QuestData questData)
    {
        if (!_runtimeStateTable.TryGetValue(questData.Id, out QuestRunTimeState state))
        {
            // NOTSTARTED 퀘스트
            return 2;
        }

        return state.Status switch
        {
            QuestStatusType.CANCOMPLETE => 0,
            QuestStatusType.INPROGRESS => 1,
            QuestStatusType.NOTSTARTED => 2,
            QuestStatusType.COMPLETED => 99,
            _ => 99
        };
    }
    bool IsMatchedLogFilter(QuestRunTimeState state, QuestLogFilterType filter)
    {
        return filter switch
        {
            QuestLogFilterType.ALL => QuestStateMachine.IsActiveQuest(state),
            QuestLogFilterType.INPROGRESS => state.Status == QuestStatusType.INPROGRESS,
            QuestLogFilterType.CANCOMPLETE => state.Status == QuestStatusType.CANCOMPLETE,
            QuestLogFilterType.COMPLETED => state.Status == QuestStatusType.COMPLETED,
            _ => true
        };
    }

    int GetQuestLogPriority(QuestRunTimeState state)
    {
        return state.Status switch
        {
            QuestStatusType.CANCOMPLETE => 0,
            QuestStatusType.INPROGRESS => 1,
            QuestStatusType.NOTSTARTED => 2,
            QuestStatusType.COMPLETED => 3,
            _ => 99
        };
    }
    bool CanShowCompletedQuestAgain(QuestData questData)
    {
        if (questData.QuestType != QuestType.REPEATABLE)
            return false;

        return questData.RepeatRule == RepeatRuleType.ALWAYS;
    }
}
