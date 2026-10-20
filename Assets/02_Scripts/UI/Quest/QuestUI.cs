using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
class QuestTable
{
    [SerializeField] List<QuestData> _questDatas = new();
    public List<QuestData> Datas => _questDatas;
}

public class QuestUI : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] GameObject _root;
    [SerializeField] GameObject _slotParent;
    [SerializeField] GameObject _questSlotPrefab;
    [SerializeField] Button     _closeBtn;


    [Header("Linked")]
    [SerializeField] string _linkedID = "NPC_QUEST";

    void Awake()
    {
        if(_root == null) _root = transform.GetChild(0).gameObject;
        if (_questSlotPrefab == null) Debug.Log("Prefab is NULL");
        if (_slotParent == null) Debug.Log("Slot Parent is NULL");
        if (_closeBtn == null) _closeBtn = GetComponentInChildren<Button>();

        if (_closeBtn != null) _closeBtn.onClick.AddListener(() => Hide());

        QuestEvents.QuestCompletedEvent     += HandleQuestCompleted;
        QuestEvents.QuestGiveUpEvent        += HandleQuestGiveUp;
        QuestEvents.QuestAcceptedEvent      += HandleQuestAccepted;
        QuestEvents.QuestUnLockEvent        += HandleQuestUnLock;
    }
    void Start()
    {
        Hide();
    }
    void OnDestroy()
    {
        if(_closeBtn != null) _closeBtn.onClick.RemoveAllListeners();

        QuestEvents.QuestCompletedEvent     -= HandleQuestCompleted;
        QuestEvents.QuestGiveUpEvent        -= HandleQuestGiveUp;
        QuestEvents.QuestAcceptedEvent      -= HandleQuestAccepted;
        QuestEvents.QuestUnLockEvent        -= HandleQuestUnLock;
    }
    void Show()
    {
        _root.SetActive(true);
    }
    
    void ClearSlots()
    {
        for (int i = _slotParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_slotParent.transform.GetChild(i).gameObject);
        }
    }
    void RefreshCurrentNpcQuests()
    {
        if (string.IsNullOrEmpty(_linkedID))
            return;

        List<QuestData> quests = QuestManager.Instance.QuestQueryService.GetQuestsForNpc(_linkedID);

        if (quests.Count == 0)
        {
            Hide();
            return;
        }

        Show(quests);
    }

    public void Hide()
    {
        _root.SetActive(false);
        UIEvents.RaiseInventoryClosed();
        UIEvents.RaiseCursorHide();
        UIEvents.RaiseUIClosed();
    }
    public void Show(IReadOnlyList<QuestData> quests)
    {
        ClearSlots();

        foreach (QuestData questData in quests)
        {
            GameObject obj = Instantiate(_questSlotPrefab, _slotParent.transform);
            QuestSlot slot = obj.GetComponent<QuestSlot>();
            slot.Init(questData);
        }

        Show();
    }
    void HandleQuestUnLock(QuestData data)
    {
        GameObject obj = Instantiate(_questSlotPrefab, _slotParent.transform);
        QuestSlot slot = obj.GetComponent<QuestSlot>();
        slot.Init(data);
    }
    void HandleQuestCompleted(QuestData data)
    {
        RefreshCurrentNpcQuests();
    }

    void HandleQuestGiveUp(QuestData data)
    {
        RefreshCurrentNpcQuests();
    }

    void HandleQuestAccepted(QuestData data)
    {
        RefreshCurrentNpcQuests();
    }
}
