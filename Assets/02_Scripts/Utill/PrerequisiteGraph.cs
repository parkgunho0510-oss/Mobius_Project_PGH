using System.Collections.Generic;

public class PrerequisiteGraph
{
    readonly Dictionary<string, List<string>> _requiredQuestIdsByQuestId = new();
    readonly Dictionary<string, List<string>> _unlockedQuestIdsByRequiredQuestId = new();

    public void Build(IEnumerable<QuestData> questDatas)
    {
        _requiredQuestIdsByQuestId.Clear();
        _unlockedQuestIdsByRequiredQuestId.Clear();

        foreach (QuestData questData in questDatas)
        {
            if (questData == null)
                continue;

            if (questData.RequiredQuestIds == null || questData.RequiredQuestIds.Count == 0)
                continue;

            _requiredQuestIdsByQuestId[questData.Id] = questData.RequiredQuestIds;

            foreach (string requiredQuestId in questData.RequiredQuestIds)
            {
                if (string.IsNullOrEmpty(requiredQuestId))
                    continue;

                if (!_unlockedQuestIdsByRequiredQuestId.TryGetValue(requiredQuestId, out List<string> unlockedQuestIds))
                {
                    unlockedQuestIds = new List<string>();
                    _unlockedQuestIdsByRequiredQuestId.Add(requiredQuestId, unlockedQuestIds);
                }

                if (!unlockedQuestIds.Contains(questData.Id))
                    unlockedQuestIds.Add(questData.Id);
            }
        }
    }

    public bool IsUnlocked(
        string questId,
        IReadOnlyDictionary<string, QuestRunTimeState> runtimeStateTable)
    {
        if (!_requiredQuestIdsByQuestId.TryGetValue(questId, out List<string> requiredQuestIds))
            return true;

        foreach (string requiredQuestId in requiredQuestIds)
        {
            if (!runtimeStateTable.TryGetValue(requiredQuestId, out QuestRunTimeState requiredState))
                return false;

            if (!IsQuestCompletedAtLeastOnce(requiredState))
                return false;
        }

        return true;
    }

    bool IsQuestCompletedAtLeastOnce(QuestRunTimeState state)
    {
        if (state == null)
            return false;

        return state.Status == QuestStatusType.COMPLETED ||
               state.CompletedCount > 0;
    }

    public IReadOnlyList<string> GetUnlockedQuestIdsByCompletedQuest(string completedQuestId)
    {
        if (_unlockedQuestIdsByRequiredQuestId.TryGetValue(completedQuestId, out List<string> unlockedQuestIds))
            return unlockedQuestIds;

        return System.Array.Empty<string>();
    }
}