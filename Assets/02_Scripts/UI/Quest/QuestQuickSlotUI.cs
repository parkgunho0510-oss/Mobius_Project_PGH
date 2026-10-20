using TMPro;
using UnityEngine;

public class QuestQuickSlotUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] QuestData _data;

    [Header("Ref")]
    [SerializeField] GameObject _root;
    [SerializeField] TMP_Text _questTitle;
    void Awake()
    {
        if (_root == null) _root = transform.GetChild(0).gameObject;
        
        if(_data == null) _questTitle.SetText("");

        Hide();
        QuestEvents.QuestAcceptedEvent          += HandleQuestAccepted;
        QuestEvents.QuestGiveUpEvent            += HandleQuestGiveUp;
        QuestEvents.QuestCompletedEvent         += HandleQuestCompleted;
        QuestEvents.QuestSaveDataLoadedEvent    += HandleQuestSaveDataLoaded;
    }
    void OnDestroy()
    {
        QuestEvents.QuestAcceptedEvent          -= HandleQuestAccepted;
        QuestEvents.QuestGiveUpEvent            -= HandleQuestGiveUp;
        QuestEvents.QuestCompletedEvent         -= HandleQuestCompleted;
        QuestEvents.QuestSaveDataLoadedEvent    -= HandleQuestSaveDataLoaded;
    }
    void Start()
    {
        RefreshFromRuntimeStates();
    }
    void Show()
    {
        _root.SetActive(true);
    }   
    void Hide()
    {
        _root.SetActive(false);
    }
    void RefreshFromRuntimeStates()
    {
        if (QuestManager.Instance == null)
        {
            Clear();
            return;
        }

        foreach (QuestRunTimeState state in QuestManager.Instance.QuestRunTimeStateTable.Values)
        {
            if (!QuestStateMachine.IsVisibleInQuickSlot(state)) continue;

            _data = state.Data;
            RefreshUI();
            return;
        }

        Clear();
    }
    void RefreshUI()
    {
        if (_data == null)
        {
            Clear();
            return;
        }

        _questTitle.SetText($"{_data.QuestName}");
        Show();
    }
    void Clear()
    {
        _data = null;
        _questTitle.SetText("");
        Hide();
    }
    void HandleQuestAccepted(QuestData data)
    {
        if (data == null) return;

        _data = data;
        RefreshUI();
    }
    void HandleQuestGiveUp(QuestData data)
    {
        if (_data == null)
        {
            RefreshFromRuntimeStates();
            return;
        }

        if (_data.Id != data.Id) return;

        RefreshFromRuntimeStates();
    }
    void HandleQuestCompleted(QuestData data)
    {
        if (_data == null)
        {
            RefreshFromRuntimeStates();
            return;
        }

        if (_data.Id != data.Id) return;

        RefreshFromRuntimeStates();
    }

    void HandleQuestSaveDataLoaded(QuestData data, QuestStatusType type)
    {
        if (QuestStateMachine.IsVisibleInQuickSlot(type)) HandleQuestAccepted(data);
    }
}
