using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Ref")]
    [SerializeField] Inventory _inventory;
    [SerializeField] PlayerStatus _playerStatus;
    [SerializeField] Wallet _wallet;
    [SerializeField] QuestUI _questUI;

    QuestRegistry _questRegistry;
    QuestQueryService _questQueryService;
    PrerequisiteGraph _prerequisiteGraph;
    Dictionary<string, QuestData> _questTable = new();
    Dictionary<string, QuestRunTimeState> _questRuntimeStateTable = new();

    public IReadOnlyDictionary<string, QuestRunTimeState> QuestRunTimeStateTable => _questRuntimeStateTable;
    public QuestRegistry QuestRegistry => _questRegistry;
    public QuestQueryService QuestQueryService => _questQueryService;
    public PrerequisiteGraph PrerequisiteGraph => _prerequisiteGraph;
    void Awake()
    {
        QuestEvents.QuestAcceptedEvent  += HandleQuestAccepted;
        QuestEvents.QuestGiveUpEvent    += HandleQuestGiveUp;
        QuestEvents.QuestCompletedEvent += HandleQuestCompleted;
        QuestEvents.KillMonsterEvent    += HandleKillMonster;
        QuestEvents.TalkToNpcEvent      += HandleTalkToNpc;
        QuestEvents.CollectItemEvent    += HandleCollectItem;
        SceneManager.sceneLoaded        += HandleSceneLoaded;

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        InitQuestQuerySystem();
    }
    void OnDestroy()
    {
        QuestEvents.QuestAcceptedEvent  -= HandleQuestAccepted;
        QuestEvents.QuestGiveUpEvent    -= HandleQuestGiveUp;
        QuestEvents.QuestCompletedEvent -= HandleQuestCompleted;
        QuestEvents.KillMonsterEvent    -= HandleKillMonster;
        QuestEvents.TalkToNpcEvent      -= HandleTalkToNpc;
        QuestEvents.CollectItemEvent    -= HandleCollectItem;
        SceneManager.sceneLoaded        -= HandleSceneLoaded;
    }
    void InitQuestQuerySystem()
    {
        IEnumerable<QuestData> quests = DataManager.Instance.quest.Table.Values;

        QuestValidator validator = new QuestValidator();
        List<string> errors = validator.Validate(quests);

        foreach (string error in errors)
        {
            Debug.LogWarning($"[QuestValidator] {error}");
        }

        _questRegistry = new QuestRegistry();
        _questRegistry.Build(quests);

        _prerequisiteGraph = new PrerequisiteGraph();
        _prerequisiteGraph.Build(quests);

        _questQueryService = new QuestQueryService(_questRegistry, _questRuntimeStateTable, _prerequisiteGraph);
    }
    void RefreshObjectiveProgressOnAccept(QuestRunTimeState state)
    {
        QuestData data = state.Data;

        for (int i = 0; i < data.Objectives.Count; i++)
        {
            QuestObjectiveData objectiveData = data.Objectives[i];
            ObjectiveRunTimeState objectiveState = state.ObjectiveRunTimeStateList[i];

            objectiveState.CurrentAmount = 0;
            objectiveState.IsCompleted = false;

            if (objectiveData.Type == QuestObjectiveType.COLLECT_ITEM)
            {
                int index = _inventory.FindIndex(objectiveData.TargetID);

                if (index != -1)
                {
                    objectiveState.CurrentAmount = _inventory.GetCurrentAmount(index);

                    if (objectiveState.CurrentAmount >= objectiveData.RequireAmount)
                    {
                        objectiveState.IsCompleted = true;
                        Debug.Log($"{objectiveData.Description} Quest Completed");
                    }
                }
            }
        }
    }
    void GenerateRunTimeState(QuestData data)
    {
        QuestRunTimeState questRunTimeState = new();
        questRunTimeState.Data = data;
        questRunTimeState.Status = QuestStatusType.NOTSTARTED;

        if (!QuestStateMachine.TryAccept(questRunTimeState))
        {
            Debug.LogWarning($"{data.Id} Quest Don't Accept");
            return;
        }


        for (int i = 0; i < data.Objectives.Count; i++)
        {
            int amount = 0;
            bool wasCompleted = false;

            if (data.Objectives[i].Type == QuestObjectiveType.COLLECT_ITEM)
            {
                int index = _inventory.FindIndex(data.Objectives[i].TargetID);

                if (index != -1)
                {
                    amount = _inventory.GetCurrentAmount(index);

                    if (amount >= data.Objectives[i].RequireAmount)
                    {
                        wasCompleted = true;
                        Debug.Log($"{data.Objectives[i].Description} Quest Completed");
                    }
                }
            }

            ObjectiveRunTimeState objRunTimeState = new();
            objRunTimeState.CurrentAmount = amount;
            objRunTimeState.IsCompleted = wasCompleted;

            questRunTimeState.ObjectiveRunTimeStateList.Add(objRunTimeState);

        }

        _questRuntimeStateTable[data.Id] = questRunTimeState;
        CheckCanComplete(questRunTimeState);
    }
    void AddObjectiveProgress(QuestObjectiveData objectiveData, ObjectiveRunTimeState objectiveState, int amount)
    {
        objectiveState.CurrentAmount += amount;

        if (objectiveState.CurrentAmount >= objectiveData.RequireAmount)
        {
            objectiveState.CurrentAmount = objectiveData.RequireAmount;
            objectiveState.IsCompleted = true;

            Debug.Log($"{objectiveData.Description} Quest Completed");
        }
    }
    void UpdateProgress(QuestObjectiveEvent objectiveEvent)
    {
        foreach (QuestRunTimeState state in _questRuntimeStateTable.Values)
        {
            if (!QuestStateMachine.CanUpdateProgress(state))
                continue;

            List<QuestObjectiveData> objectiveDatas = state.Data.Objectives;

            for (int i = 0; i < objectiveDatas.Count; i++)
            {
                QuestObjectiveData objectiveData = objectiveDatas[i];
                ObjectiveRunTimeState objectiveRunTimeState = state.ObjectiveRunTimeStateList[i];

                if (!QuestObjectiveMatcher.IsMatch(objectiveData, objectiveRunTimeState, objectiveEvent)) continue;

                AddObjectiveProgress(objectiveData, objectiveRunTimeState, objectiveEvent.Amount);
            }

            CheckCanComplete(state);
        }
    }
    void CheckCanComplete(QuestRunTimeState state)
    {
        if (!QuestStateMachine.CanUpdateProgress(state)) return;

        bool canComplete = true;

        foreach (var runTimeState in state.ObjectiveRunTimeStateList)
        {
            if (!runTimeState.IsCompleted)
            {
                canComplete = false;
                break;
            }
        }

        if (!canComplete) return;

        if (QuestStateMachine.TrySetCanComplete(state))
        {
            QuestEvents.RaiseQuestCanComplete(state.Data);
        }
    }
    bool HasRequiredItem(QuestObjectiveData objectiveData)
    {
        if (_inventory == null)
            return false;

        int index = _inventory.FindIndex(objectiveData.TargetID);

        if (index == -1)
            return false;

        return _inventory.GetCurrentAmount(index) >= objectiveData.RequireAmount;
    }
    void CheckBringItemObjectivesForNpc(string npcId)
    {
        foreach (QuestRunTimeState state in _questRuntimeStateTable.Values)
        {
            if (!QuestStateMachine.CanUpdateProgress(state)) continue;

            QuestData questData = state.Data;

            if (questData.CompleteNpcID != npcId) continue;

            for (int i = 0; i < questData.Objectives.Count; i++)
            {
                QuestObjectiveData objectiveData = questData.Objectives[i];
                ObjectiveRunTimeState objectiveState = state.ObjectiveRunTimeStateList[i];

                if (objectiveState.IsCompleted)
                    continue;

                if (objectiveData.Type != QuestObjectiveType.BRING_ITEM)
                    continue;

                if (!HasRequiredItem(objectiveData))
                    continue;

                objectiveState.CurrentAmount = objectiveData.RequireAmount;
                objectiveState.IsCompleted = true;
                Debug.Log($"{objectiveData.Description} Bring Item Quest Completed");
            }

            CheckCanComplete(state);
        }
    }
    bool CanConsumeBringItems(QuestData data)
    {
        foreach (QuestObjectiveData objectiveData in data.Objectives)
        {
            if (objectiveData.Type != QuestObjectiveType.BRING_ITEM)
                continue;

            if (_inventory == null)
                return false;

            int index = _inventory.FindIndex(objectiveData.TargetID);

            if (index == -1)
                return false;

            int currentAmount = _inventory.GetCurrentAmount(index);

            if (currentAmount < objectiveData.RequireAmount)
                return false;
        }

        return true;
    }
    void ConsumeBringItems(QuestData data)
    {
        foreach (QuestObjectiveData objectiveData in data.Objectives)
        {
            if (objectiveData.Type != QuestObjectiveType.BRING_ITEM)
                continue;

            int index = _inventory.FindIndex(objectiveData.TargetID);

            if (index == -1)
                continue;

            _inventory.Remove(index, objectiveData.RequireAmount);
        }
    }
    void GiveRewards(QuestData data)
    {
        foreach (QuestRewardData reward in data.Rewards)
        {
            switch (reward.Type)
            {
                case QuestRewardType.GOLD:
                    _wallet.AddGold(reward.Amount);
                    break;

                case QuestRewardType.EXP:
                    _playerStatus.AddExp(reward.Amount);
                    break;

                case QuestRewardType.CONSUMABLE_ITEM:
                    _inventory.Add(DataManager.Instance.GetConsumableItem(reward.RewardID), reward.Amount);
                    break;

                case QuestRewardType.EQUIPMENT_ITEM:
                    _inventory.Add(DataManager.Instance.GetWeapon(reward.RewardID));
                    break;

                case QuestRewardType.REPUTATION:
                    QuestEvents.RaiseReputationUpdated(reward.RewardID, reward.Amount);
                    break;

                case QuestRewardType.UNLOCK_QUEST:
                    QuestData unlockQuest = DataManager.Instance.GetQuest(reward.RewardID);

                    if (unlockQuest != null)
                    {
                        QuestEvents.RaiseQuestUnLock(unlockQuest);
                    }

                    break;
            }
        }
    }
    void CompleteQuest(QuestData data)
    {
        if (!_questRuntimeStateTable.TryGetValue(data.Id, out QuestRunTimeState state))
            return;

        if (!QuestStateMachine.CanComplete(state))
        {
            Debug.LogWarning($"{data.Id} Quest can't Complete");
            return;
        }

        if (!CanConsumeBringItems(data))
        {
            Debug.LogWarning($"{data.Id} Quest can't Complete. Required bring items are missing.");
            return;
        }

        ConsumeBringItems(data);
        GiveRewards(data);

        if (!QuestStateMachine.TryComplete(state))
        {
            Debug.LogWarning($"{data.Id} Quest complete transition failed.");
            return;
        }
        HandleQuestAfterComplete(data, state);
    }
    public void OpenQuestUI(string npcId)
    {
        if (_questQueryService == null)
        {
            Debug.LogWarning("QuestQueryService is not Initialize.");
            return;
        }

        if (_questUI == null)
        {
            _questUI = FindFirstObjectByType<QuestUI>();

            if (_questUI == null)
            {
                Debug.LogWarning("QuestUI is not connected");
                return;
            }
        }

        List<QuestData> quests = _questQueryService.GetQuestsForNpc(npcId);
        if (quests.Count == 0)
        {
            _questUI.Hide();
            return;
        }

        _questUI.Show(quests);
    }
    void UpdateTalkToNpcProgress(string npcId)
    {
        foreach (QuestRunTimeState state in _questRuntimeStateTable.Values)
        {
            if (!QuestStateMachine.CanUpdateProgress(state))
                continue;

            QuestData questData = state.Data;

            bool hasBringItemObjective = HasBringItemObjective(questData);
            bool hasTalkToThisNpcObjective = HasTalkToNpcObjective(questData, npcId);

            if (hasBringItemObjective && hasTalkToThisNpcObjective)
            {
                TryCompleteBringItemDeliveryObjectives(state, npcId);
            }
            else
            {
                UpdateNormalTalkToNpcObjective(state, npcId);
            }

            CheckCanComplete(state);
        }
    }
    bool HasBringItemObjective(QuestData questData)
    {
        foreach (QuestObjectiveData objectiveData in questData.Objectives)
        {
            if (objectiveData.Type == QuestObjectiveType.BRING_ITEM)
                return true;
        }

        return false;
    }
    bool HasTalkToNpcObjective(QuestData questData, string npcId)
    {
        foreach (QuestObjectiveData objectiveData in questData.Objectives)
        {
            if (objectiveData.Type != QuestObjectiveType.TALK_TO_NPC)
                continue;

            if (objectiveData.TargetID == npcId)
                return true;
        }

        return false;
    }
    bool HasAllRequiredBringItems(QuestData questData)
    {
        foreach (QuestObjectiveData objectiveData in questData.Objectives)
        {
            if (objectiveData.Type != QuestObjectiveType.BRING_ITEM)
                continue;

            if (!HasRequiredItem(objectiveData))
                return false;
        }

        return true;
    }
    void TryCompleteBringItemDeliveryObjectives(QuestRunTimeState state, string npcId)
    {
        QuestData questData = state.Data;

        if (!HasAllRequiredBringItems(questData))
        {
            Debug.Log($"{questData.Id} Bring Item Quest - Required item is missing.");
            return;
        }

        for (int i = 0; i < questData.Objectives.Count; i++)
        {
            QuestObjectiveData objectiveData = questData.Objectives[i];
            ObjectiveRunTimeState objectiveState = state.ObjectiveRunTimeStateList[i];

            if (objectiveState.IsCompleted)
                continue;

            if (objectiveData.Type == QuestObjectiveType.BRING_ITEM)
            {
                objectiveState.CurrentAmount = objectiveData.RequireAmount;
                objectiveState.IsCompleted = true;

                Debug.Log($"{objectiveData.Description} Quest Completed");
            }
            else if (objectiveData.Type == QuestObjectiveType.TALK_TO_NPC &&
                     objectiveData.TargetID == npcId)
            {
                objectiveState.CurrentAmount = objectiveData.RequireAmount;
                objectiveState.IsCompleted = true;

                Debug.Log($"{objectiveData.Description} Quest Completed");
            }
        }
    }
    void UpdateNormalTalkToNpcObjective(QuestRunTimeState state, string npcId)
    {
        QuestObjectiveEvent objectiveEvent =
            new QuestObjectiveEvent(QuestObjectiveType.TALK_TO_NPC, npcId, 1);

        List<QuestObjectiveData> objectiveDatas = state.Data.Objectives;

        for (int i = 0; i < objectiveDatas.Count; i++)
        {
            QuestObjectiveData objectiveData = objectiveDatas[i];
            ObjectiveRunTimeState objectiveState = state.ObjectiveRunTimeStateList[i];

            if (!QuestObjectiveMatcher.IsMatch(objectiveData, objectiveState, objectiveEvent))
                continue;

            AddObjectiveProgress(objectiveData, objectiveState, objectiveEvent.Amount);
        }
    }
    public List<QuestSaveData> CreateSaveData()
    {
        List<QuestSaveData> questSaveDatas = new();

        foreach (QuestData questData in _questTable.Values)
        {
            if (!_questRuntimeStateTable.TryGetValue(questData.Id, out QuestRunTimeState runtimeState))
                continue;

            QuestSaveData questSaveData = new();
            questSaveData.QuestID = questData.Id;
            questSaveData.QuestStatusType = runtimeState.Status;
            questSaveData.CompletedCount = runtimeState.CompletedCount;
            questSaveData.LastCompletedAt = runtimeState.LastCompletedAt;

            List<QuestObjectiveSaveData> questObjectiveSaveDatas = new();

            foreach (ObjectiveRunTimeState runTimeState in runtimeState.ObjectiveRunTimeStateList)
            {
                QuestObjectiveSaveData questObjectiveSaveData = new();
                questObjectiveSaveData.CurrentAmount = runTimeState.CurrentAmount;
                questObjectiveSaveData.IsCompleted = runTimeState.IsCompleted;
                questObjectiveSaveDatas.Add(questObjectiveSaveData);
            }

            questSaveData.ObjectiveSaveDatas = questObjectiveSaveDatas;
            questSaveDatas.Add(questSaveData);
        }

        return questSaveDatas;
    }
    public void LoadSaveData(List<QuestSaveData> dataList)
    {
        _questTable.Clear();
        _questRuntimeStateTable.Clear();

        if (dataList == null) return;
        if (dataList.Count == 0) return;

        for (int i = 0; i < dataList.Count; i++)
        {
            QuestSaveData saveData = dataList[i];

            QuestData questData = DataManager.Instance.GetQuest(saveData.QuestID);

            if (questData == null)
            {
                Debug.LogWarning($"{saveData.QuestID} QuestData is null.");
                continue;
            }

            QuestRunTimeState questRunTimeState = new();
            questRunTimeState.Data = questData;
            questRunTimeState.Status = saveData.QuestStatusType;
            questRunTimeState.CompletedCount = saveData.CompletedCount;
            questRunTimeState.LastCompletedAt = saveData.LastCompletedAt;

            for (int j = 0; j < saveData.ObjectiveSaveDatas.Count; j++)
            {
                QuestObjectiveSaveData objSaveData = saveData.ObjectiveSaveDatas[j];

                ObjectiveRunTimeState objectiveRunTimeState = new();
                objectiveRunTimeState.CurrentAmount = objSaveData.CurrentAmount;
                objectiveRunTimeState.IsCompleted = objSaveData.IsCompleted;

                questRunTimeState.ObjectiveRunTimeStateList.Add(objectiveRunTimeState);
            }

            _questTable[saveData.QuestID] = questData;
            _questRuntimeStateTable[saveData.QuestID] = questRunTimeState;

            QuestEvents.RaiseQuestSaveDataLoaded(questData, saveData.QuestStatusType);
        }

        foreach (QuestRunTimeState state in _questRuntimeStateTable.Values)
        {
            CheckCanComplete(state);
        }
    }
    public void ResetForNewGame()
    {
        _questTable.Clear();
        _questRuntimeStateTable.Clear();

        InitQuestQuerySystem();

        Debug.Log("[QuestManager] New Game Quest Runtime Cleared");
    }
    void HandleQuestAccepted(QuestData data)
    {
        if (_questRuntimeStateTable.TryGetValue(data.Id, out QuestRunTimeState existingState))
        {
            if (!QuestStateMachine.TryAccept(existingState))
            {
                Debug.LogWarning($"{data.Id} Quest can't Accept. Current State: {existingState.Status}");
                return;
            }

            RefreshObjectiveProgressOnAccept(existingState);
            CheckCanComplete(existingState);
            return;
        }
        _questTable[data.Id] = data;
        GenerateRunTimeState(data);
    }
    void HandleQuestGiveUp(QuestData data)
    {
        if (!_questRuntimeStateTable.TryGetValue(data.Id, out QuestRunTimeState state))
            return;

        if (!QuestStateMachine.TryGiveUp(state))
        {
            Debug.LogWarning($"{data.Id} Quest can't GiveUp");
            return;
        }



        // TODO : Fixed
        if (_questTable.ContainsKey(data.Id))
        {
            _questTable.Remove(data.Id);
        }
        if (_questRuntimeStateTable.ContainsKey(data.Id))
        {
            _questRuntimeStateTable.Remove(data.Id);
        }
    }
    void HandleQuestCompleted(QuestData data)
    {
        CompleteQuest(data);
    }
    void HandleTalkToNpc(NpcData npcData)
    {
        if (npcData == null) return;

        UpdateTalkToNpcProgress(npcData.NpcID);
    }
    void HandleCollectItem(ItemData itemData, int amount)
    {
        if (itemData == null) return;

        UpdateProgress(new QuestObjectiveEvent(QuestObjectiveType.COLLECT_ITEM, itemData.ID, amount));
    }
    void HandleKillMonster(MonsterDB monsterData)
    {
        if (monsterData == null) return;

        UpdateProgress(new QuestObjectiveEvent(QuestObjectiveType.KILL_MONSTER, monsterData.ID, 1));
    }
    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Init") return;
        if (scene.name == "StartScene_LCS") return;

        _inventory = FindFirstObjectByType<Inventory>();
        _playerStatus = FindFirstObjectByType<PlayerStatus>();
        _questUI = FindFirstObjectByType<QuestUI>();
        _wallet = FindFirstObjectByType<Wallet>();
    }
    void HandleQuestAfterComplete(QuestData data, QuestRunTimeState state)
    {
        state.CompletedCount++;
        state.LastCompletedAt = DateTime.UtcNow.ToString("o");

        if (_prerequisiteGraph != null)
        {
            IReadOnlyList<string> unlockedQuestIds =
                _prerequisiteGraph.GetUnlockedQuestIdsByCompletedQuest(data.Id);

            foreach (string unlockedQuestId in unlockedQuestIds)
            {
                if (_prerequisiteGraph.IsUnlocked(unlockedQuestId, _questRuntimeStateTable))
                {
                    Debug.Log($"[Quest Unlock] {unlockedQuestId} is now available.");
                }
            }
        }

        if (data.QuestType == QuestType.REPEATABLE &&
            data.RepeatRule == RepeatRuleType.ALWAYS)
        {
            QuestStateMachine.TryResetToNotStarted(state);
        }
    }
}
