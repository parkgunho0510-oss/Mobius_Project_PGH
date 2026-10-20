using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
public class QuestLogUI : MonoBehaviour
{
    [SerializeField] GameObject _root;
    [SerializeField] GameObject _slotPrefab;
    [SerializeField] GameObject _slotParent;
    [SerializeField] Button _closeBtn;
    [SerializeField] GameObject _detailPanel;

    Dictionary<string, QuestData>   _questDataTable = new();
    Dictionary<string, GameObject>  _logSlotTable = new();

    void Awake()
    {
        if (_root == null)          _root = transform.GetChild(0).gameObject;
        if (_slotPrefab == null)    Debug.Log("Slot Prefab is NULL");
        if (_slotParent == null)    Debug.Log("Slot Parent is NULL");
        if (_closeBtn == null)      Debug.Log("Close Btn is NULL");
        if (_detailPanel == null)   Debug.Log("Detail Panel is NULL");
        _questDataTable.Clear();
        QuestEvents.QuestAcceptedEvent          += HandleQuestAccepted;
        QuestEvents.QuestGiveUpEvent            += HandleQuestGiveUp;
        QuestEvents.QuestCompletedEvent         += HandleQuestCompleted;
        QuestEvents.QuestLogOpenedEvent         += HandleQuestLogOpened;
        QuestEvents.QuestLogClosedEvent         += HandleQuestLogClosed;
        QuestEvents.QuestSaveDataLoadedEvent    += HandleQuestSaveDataLoaded;
        _closeBtn.onClick.AddListener(() => Hide());
    }
    void OnDestroy()
    {
        QuestEvents.QuestAcceptedEvent          -= HandleQuestAccepted;
        QuestEvents.QuestGiveUpEvent            -= HandleQuestGiveUp;
        QuestEvents.QuestCompletedEvent         -= HandleQuestCompleted;
        QuestEvents.QuestLogOpenedEvent         -= HandleQuestLogOpened;
        QuestEvents.QuestLogClosedEvent         -= HandleQuestLogClosed;
        QuestEvents.QuestSaveDataLoadedEvent    -= HandleQuestSaveDataLoaded;
        _closeBtn.onClick.RemoveAllListeners();
    }

    void Start()
    {
        RefreshFromRuntimeStates();
        Hide();
    }

    void Show()
    {
        _root.SetActive(true);
    }

    void Hide()
    {
        UIEvents.RaiseInventoryClosed();
        UIEvents.RaiseCursorHide();
        UIEvents.RaiseUIClosed();
        _root.SetActive(false);
    }
    void RemoveLogSlot(QuestData data)
    {
        if (data == null)
            return;

        _questDataTable.Remove(data.Id);

        if (_logSlotTable.TryGetValue(data.Id, out GameObject slotObj))
        {
            if (slotObj != null)
                Destroy(slotObj);

            _logSlotTable.Remove(data.Id);
        }
    }
    void CreateLogSlot(QuestData data)
    {
        if (data == null)
            return;

        if (_logSlotTable.ContainsKey(data.Id))
            return;

        GameObject obj = Instantiate(_slotPrefab, _slotParent.transform);

        QuestLogSlot logSlot;

        if (!obj.TryGetComponent(out logSlot))
        {
            logSlot = obj.AddComponent<QuestLogSlot>();
        }

        logSlot.Init(data);

        _questDataTable[data.Id] = data;
        _logSlotTable[data.Id] = logSlot.gameObject;

        QuestEvents.QuestCanCompleteEvent += logSlot.HandleQuestCanComplete;
    }
    void ClearSlots()
    {
        foreach (GameObject slotObj in _logSlotTable.Values)
        {
            if (slotObj != null)
                Destroy(slotObj);
        }

        _questDataTable.Clear();
        _logSlotTable.Clear();
    }
    void RefreshFromRuntimeStates()
    {
        ClearSlots();

        if (QuestManager.Instance == null)
            return;

        foreach (QuestRunTimeState state in QuestManager.Instance.QuestRunTimeStateTable.Values)
        {
            if (!QuestStateMachine.IsVisibleInQuestLog(state))
                continue;

            CreateLogSlot(state.Data);
        }
    }
    void HandleQuestLogOpened()
    {
        RefreshFromRuntimeStates();
        Show();
    }
    void HandleQuestLogClosed()
    {
        Hide();
    }
    void HandleQuestAccepted(QuestData data)
    {
        CreateLogSlot(data);
    }
    void HandleQuestGiveUp(QuestData data)
    {
        RemoveLogSlot(data);
    }
    void HandleQuestCompleted(QuestData data)
    {
        RemoveLogSlot(data);
    }
    void HandleQuestSaveDataLoaded(QuestData data, QuestStatusType type)
    {
        if(QuestStateMachine.IsVisibleInQuestLog(type)) HandleQuestAccepted(data);
    }
}

