using System;
using System.Collections.Generic;
using UnityEngine;
public class QuestRegistry
{
    readonly Dictionary<string, QuestData> _questById = new();
    readonly Dictionary<string, List<QuestData>> _questsByNpcId = new();
    readonly Dictionary<QuestType, List<QuestData>> _questsByType = new();
    readonly Dictionary<QuestObjectiveType, List<QuestData>> _questsByObjectiveType = new();

    public IReadOnlyDictionary<string, QuestData> QuestById => _questById;
    void AddToTable<T>(Dictionary<T, List<QuestData>> table, T key, QuestData questData)
    {
        if (!table.TryGetValue(key, out List<QuestData> list))
        {
            list = new List<QuestData>();
            table.Add(key, list);
        }

        if (!list.Contains(questData))
        {
            list.Add(questData);
        }
    }
    void RegisterQuest(QuestData questData)
    {
        if (string.IsNullOrEmpty(questData.Id))
        {
            Debug.LogWarning("QuestData's Id is NULL");
            return;
        }

        if (_questById.ContainsKey(questData.Id))
        {
            Debug.LogWarning($"{questData.Id} already Contains");
            return;
        }
        _questById.Add(questData.Id, questData);
        AddToTable(_questsByType, questData.QuestType, questData);

        if (!string.IsNullOrEmpty(questData.StartNpcID))
        {
            AddToTable(_questsByNpcId, questData.StartNpcID, questData);
        }
        
        if (!string.IsNullOrEmpty(questData.CompleteNpcID) && questData.CompleteNpcID != questData.StartNpcID)
        {
            AddToTable(_questsByNpcId, questData.CompleteNpcID, questData);
        }

        if (questData.Objectives == null)
            return;

        foreach (QuestObjectiveData objectiveData in questData.Objectives)
        {
            if (objectiveData == null)
                continue;

            AddToTable(_questsByObjectiveType, objectiveData.Type, questData);
        }
    }
    
    public void ClearAll()
    {
        _questById.Clear();
        _questsByNpcId.Clear();
        _questsByType.Clear();
        _questsByObjectiveType.Clear();
    }

    public void Build(IEnumerable<QuestData> questDatas)
    {
        ClearAll();

        foreach (var questData in questDatas)
        {
            if(questData == null) continue;

            RegisterQuest(questData);
        }
    }
    public bool TryGetQuest(string questId, out QuestData questData)
    {
        return _questById.TryGetValue(questId, out questData);
    }

    public QuestData GetQuest(string questId)
    {
        if (_questById.TryGetValue(questId, out QuestData questData))
            return questData;

        Debug.LogWarning($" {questId} not contain Registry ");
        return null;
    }

    public IReadOnlyList<QuestData> GetQuestsByNpcId(string npcId)
    {
        if (_questsByNpcId.TryGetValue(npcId, out List<QuestData> quests))
            return quests;

        return Array.Empty<QuestData>();
    }

    public IReadOnlyList<QuestData> GetQuestsByType(QuestType questType)
    {
        if (_questsByType.TryGetValue(questType, out List<QuestData> quests))
            return quests;

        return Array.Empty<QuestData>();
    }

    public IReadOnlyList<QuestData> GetQuestsByObjectiveType(QuestObjectiveType objectiveType)
    {
        if (_questsByObjectiveType.TryGetValue(objectiveType, out List<QuestData> quests))
            return quests;

        return Array.Empty<QuestData>();
    }
}
