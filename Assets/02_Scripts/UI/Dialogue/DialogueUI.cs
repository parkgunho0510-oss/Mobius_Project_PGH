using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] Image _portrait;
    [SerializeField] TMP_Text _text;
    [SerializeField] Button _button;
    [SerializeField] GameObject _root;

    NpcData _data;
    DialogueData _dialogueData;
    [SerializeField] int _currentNodeIndex = 0;
    void Awake()
    {
        if (_portrait == null) _portrait = GetComponentsInChildren<Image>()[1];
        if (_text == null) _text = GetComponentInChildren<TMP_Text>();
        if(_button == null) _button = GetComponentInChildren<Button>();
        if (_root == null) _root = transform.GetChild(0).gameObject;
        UIEvents.OnNpcInteractedEvent += HandleNpcInteracted;
        
    }
    void Start()
    {
        _button.onClick.AddListener(()=>OnClickNextBtn());
        Hide();
    }
    void OnDestroy()
    {
        _button.onClick.RemoveAllListeners();
        UIEvents.OnNpcInteractedEvent -= HandleNpcInteracted;
    }
    void HandleNpcInteracted(NpcData data, NpcRunTimeState runTimeState)
    {
        _data = data;
        _dialogueData = data.Profiles[(int)runTimeState.NpcStateType].DialogueData;

        _portrait.sprite = data.Portrait;
        _currentNodeIndex = 0;

        _text.SetText(_dialogueData.DialogueNodeDatas[_currentNodeIndex].Text);
        Show();
    }
    void OnClickNextBtn()
    {
        if (_dialogueData.DialogueNodeDatas[_currentNodeIndex].IsEnd)
        {
            UIEvents.RaiseDialogueEnded(_data.NpcID);
            Hide();

            _data = null;
            _dialogueData = null;
            _currentNodeIndex = 0;
        }
        else
        {
            _currentNodeIndex++;
            RefreshUI();

        }
    }
    void Show()
    {
        _root.SetActive(true);
    }
    void Hide()
    {
        _root.SetActive(false);
    }
    void RefreshUI()
    {
        _portrait.sprite = _data.Portrait;
        _text.SetText(_dialogueData.DialogueNodeDatas[_currentNodeIndex].Text);
    }
}
