using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuestSlot : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] QuestData _questData;

    [SerializeField] TMP_Text _title;
    [SerializeField] TMP_Text _target;
    [SerializeField] TMP_Text _reward;
    [SerializeField] Button _checkBox;
    [SerializeField] Image _check;
    [SerializeField] QuestAcceptPopup _acceptPopup;
    [SerializeField] QuestCompletePopup _completePopup;
    [SerializeField] QuestGiveUpPopup _giveUpPopup;
    
    bool _isAccept = false;
    public bool IsAccept => _isAccept;
    void Awake()
    {
        if (_title == null)     _title      = transform.GetChild(0).GetComponent<TMP_Text>();
        if (_target == null)    _target     = transform.GetChild(1).GetComponent<TMP_Text>();
        if (_reward == null)    _reward     = transform.GetChild(2).GetComponent<TMP_Text>();
        if (_checkBox == null) _checkBox    = transform.GetChild(3).GetComponent<Button>();
        if (_check == null)     _check      = _checkBox.transform.GetChild(0).GetComponent<Image>();

        if (_checkBox != null) _checkBox.onClick.AddListener(() => OnClickCheckBoxBtn());

        QuestEvents.OnAppectBtnClickEvent       += HandleOnAcceptBtnClick;
        QuestEvents.OnCompleteBtnClickEvent     += HandleOnCompleteBtnClick;
        QuestEvents.OnGiveUpBtnClickEvent       += HandleOnGiveUpBtnClick;
        QuestEvents.QuestSaveDataLoadedEvent    += HandleQuestSaveDataLoaded;
    }
    void OnDestroy()
    {
        QuestEvents.OnAppectBtnClickEvent       -= HandleOnAcceptBtnClick;
        QuestEvents.OnCompleteBtnClickEvent     -= HandleOnCompleteBtnClick;
        QuestEvents.OnGiveUpBtnClickEvent       -= HandleOnGiveUpBtnClick;
        QuestEvents.QuestSaveDataLoadedEvent    -= HandleQuestSaveDataLoaded;
    }
    void Start()
    {
        if (_acceptPopup == null)   _acceptPopup    = FindFirstObjectByType<QuestAcceptPopup>();
        if (_completePopup == null) _completePopup  = FindFirstObjectByType<QuestCompletePopup>();
        if (_giveUpPopup == null)   _giveUpPopup    = FindFirstObjectByType<QuestGiveUpPopup>();
    }
    void RefreshUI()
    {
        List<QuestObjectiveData> objDatas = _questData.Objectives;
        List<QuestRewardData> rewardDatas = _questData.Rewards;

        _title.SetText($"{_questData.QuestName}");
        _target.SetText($"목표: ");
        _reward.SetText($"보상: ");
        for (int i = 0; i < objDatas.Count; i++)
        {
            _target.text += $"{objDatas[i].Description} ";
        }
        for (int i = 0; i < rewardDatas.Count; i++)
        {
            _reward.text += $"{rewardDatas[i].Description} ";
        }
    }
    public void Init(QuestData questData)
    {
        _questData = questData;
        BindPopupRefs();
        RefreshUI();
        RefreshAcceptState();
    }
    
    void BindPopupRefs()
    {
        if (_acceptPopup == null)
            _acceptPopup = FindFirstObjectByType<QuestAcceptPopup>();

        if (_completePopup == null)
            _completePopup = FindFirstObjectByType<QuestCompletePopup>();

        if (_giveUpPopup == null)
            _giveUpPopup = FindFirstObjectByType<QuestGiveUpPopup>();
    }
    void RefreshAcceptState()
    {
        _isAccept = false;

        if (_check != null)
            _check.gameObject.SetActive(false);

        if (QuestManager.Instance == null)
            return;

        if (!QuestManager.Instance.QuestRunTimeStateTable.TryGetValue(_questData.Id, out QuestRunTimeState state))
            return;

        if (QuestStateMachine.IsActiveQuest(state))
        {
            _isAccept = true;

            if (_check != null)
            {
                _check.gameObject.SetActive(true);
            }
        }
    }
    void OnClickCheckBoxBtn()
    {
        if (!QuestManager.Instance.QuestRunTimeStateTable.TryGetValue(_questData.Id, out QuestRunTimeState state))
        {
            _acceptPopup.Show(_questData.Id);
            return;
        }

        if (QuestStateMachine.CanComplete(state))
        {
            _completePopup.Show(_questData.Id);
        }
        else if (QuestStateMachine.CanGiveUp(state))
        {
            _giveUpPopup.Show(_questData.Id);
        }
        else
        {
            _acceptPopup.Show(_questData.Id);
        }
    }
    void HandleOnAcceptBtnClick(string id)
    {
        if (_questData.Id != id) return;

        if (QuestManager.Instance.QuestRunTimeStateTable.TryGetValue(_questData.Id, out QuestRunTimeState state))
        {
            if (!QuestStateMachine.CanAccept(state))
            {
                RefreshAcceptState();
                return;
            }
        }

        QuestEvents.RaiseQuestAccepted(_questData);
        RefreshAcceptState();
    }
    void HandleOnCompleteBtnClick(string id)
    {
        if (_questData.Id != id) return;

        QuestEvents.RaiseQuestCompleted(_questData);

        Destroy(gameObject);
        this.enabled = false;
    }
    void HandleOnGiveUpBtnClick(string id)
    {
        if (_questData.Id != id) return;
        _isAccept = false;
        _check.gameObject.SetActive(false);
        QuestEvents.RaiseQuestGiveUp(_questData);
    }
    void HandleQuestSaveDataLoaded(QuestData data, QuestStatusType type)
    {
        if (_questData.Id != data.Id) return;

        switch (type)
        {
            case QuestStatusType.NONE: break;
            case QuestStatusType.NOTSTARTED: break;
            case QuestStatusType.INPROGRESS:
            case QuestStatusType.CANCOMPLETE:
                {
                    _isAccept = true;
                    _check.gameObject.SetActive(true);
                }
                break;
            case QuestStatusType.COMPLETED:
                {
                    Destroy(gameObject);
                    this.enabled = false;
                }
                break;
        }

        
    }
}
