using System.Collections.Generic;
using System.Linq;

public class QuestValidator
{
    public List<string> Validate(IEnumerable<QuestData> quests)
    {
        List<string> errors = new();

        if (quests == null)
        {
            errors.Add("QuestData 목록이 null입니다.");
            return errors;
        }

        List<QuestData> questList = quests
            .Where(q => q != null)
            .ToList();

        Dictionary<string, QuestData> questById = new();

        ValidateDuplicateIds(questList, questById, errors);

        foreach (QuestData quest in questList)
        {
            ValidateBasicFields(quest, errors);
            ValidateObjectives(quest, errors);
            ValidateRewards(quest, questById, errors);
            ValidatePrerequisites(quest, questById, errors);
        }

        ValidateCircularPrerequisites(questList, errors);

        return errors;
    }
    void ValidateDuplicateIds(List<QuestData> quests, Dictionary<string, QuestData> questById, List<string> errors)
    {
        foreach (QuestData quest in quests)
        {
            if (string.IsNullOrWhiteSpace(quest.Id))
            {
                errors.Add("Quest Id is Empty");
                continue;
            }

            if (questById.ContainsKey(quest.Id))
            {
                errors.Add($"중복 Quest Id가 있습니다. Id: {quest.Id}");
                continue;
            }

            questById.Add(quest.Id, quest);
        }
    }
    void ValidateBasicFields(QuestData quest, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(quest.QuestName))
            errors.Add($"{quest.Id}: QuestName is Empty");

        if (string.IsNullOrWhiteSpace(quest.StartNpcID))
            errors.Add($"{quest.Id}: StartNpcID is Empty");

        if (string.IsNullOrWhiteSpace(quest.CompleteNpcID))
            errors.Add($"{quest.Id}: CompleteNpcID is Empty");
    }
    void ValidateObjectives(QuestData quest, List<string> errors)
    {
        if (quest.Objectives == null || quest.Objectives.Count == 0)
        {
            errors.Add($"{quest.Id}: Objective is Empty");
            return;
        }

        foreach (QuestObjectiveData objective in quest.Objectives)
        {
            if (objective == null)
            {
                errors.Add($"{quest.Id}: null Objective가 있습니다.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(objective.TargetID))
                errors.Add($"{quest.Id}: Objective TargetID is Empty");

            if (objective.RequireAmount <= 0)
                errors.Add($"{quest.Id}: Objective RequireAmount가 0 이하입니다.");
        }
    }
    void ValidateRewards(QuestData quest, Dictionary<string, QuestData> questById, List<string> errors)
    {
        if (quest.Rewards == null || quest.Rewards.Count == 0)
        {
            errors.Add($"{quest.Id}: Reward가 없습니다.");
            return;
        }

        foreach (QuestRewardData reward in quest.Rewards)
        {
            if (reward == null)
            {
                errors.Add($"{quest.Id}: null Reward가 있습니다.");
                continue;
            }

            if (reward.Amount < 0)
                errors.Add($"{quest.Id}: Reward Amount가 음수입니다.");

            if (reward.Type == QuestRewardType.UNLOCK_QUEST)
            {
                if (string.IsNullOrWhiteSpace(reward.RewardID))
                {
                    errors.Add($"{quest.Id}: UNLOCK_QUEST RewardID가 비어 있습니다.");
                    continue;
                }

                if (reward.RewardID == quest.Id)
                    errors.Add($"{quest.Id}: 자기 자신을 UNLOCK_QUEST로 해금합니다.");

                if (!questById.ContainsKey(reward.RewardID))
                    errors.Add($"{quest.Id}: 존재하지 않는 Quest를 UNLOCK_QUEST로 해금합니다. RewardID: {reward.RewardID}");
            }
        }
    }
    void ValidatePrerequisites(QuestData quest, Dictionary<string, QuestData> questById, List<string> errors)
    {
        if (quest.RequiredQuestIds == null)
            return;

        foreach (string requiredQuestId in quest.RequiredQuestIds)
        {
            if (string.IsNullOrWhiteSpace(requiredQuestId))
                continue;

            if (requiredQuestId == quest.Id)
                errors.Add($"{quest.Id}: 자기 자신을 RequiredQuestIds로 가지고 있습니다.");

            if (!questById.ContainsKey(requiredQuestId))
                errors.Add($"{quest.Id}: 존재하지 않는 선행 QuestId입니다. RequiredQuestId: {requiredQuestId}");
        }
    }
    void ValidateCircularPrerequisites(List<QuestData> quests, List<string> errors)
    {
        Dictionary<string, QuestData> questById = quests
            .Where(q => !string.IsNullOrWhiteSpace(q.Id))
            .GroupBy(q => q.Id)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (QuestData quest in quests)
        {
            HashSet<string> visited = new();
            HashSet<string> stack = new();

            if (HasCycle(quest.Id, questById, visited, stack))
            {
                errors.Add($"{quest.Id}: RequiredQuestIds에 순환 참조가 있습니다.");
            }
        }
    }
    bool HasCycle(string questId, Dictionary<string, QuestData> questById, HashSet<string> visited, HashSet<string> stack)
    {
        if (string.IsNullOrWhiteSpace(questId))
            return false;

        if (stack.Contains(questId))
            return true;

        if (visited.Contains(questId))
            return false;

        if (!questById.TryGetValue(questId, out QuestData quest))
            return false;

        visited.Add(questId);
        stack.Add(questId);

        if (quest.RequiredQuestIds != null)
        {
            foreach (string requiredId in quest.RequiredQuestIds)
            {
                if (HasCycle(requiredId, questById, visited, stack))
                    return true;
            }
        }

        stack.Remove(questId);
        return false;
    }
}