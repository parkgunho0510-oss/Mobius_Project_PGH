using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class QuestLogSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TMP_Text _title;
    [SerializeField] Image _icon;
    [SerializeField] Sprite _inProgressSprite;
    [SerializeField] Sprite _canCompleteSprite;
    [SerializeField] QuestDetailPopup _detailPopup;

    QuestData _data = null;
    void Awake()
    {
        if (_title == null)                 Debug.Log("Title Text is NULL");
        if (_icon == null)                  Debug.Log("Icon is NULL");
        if (_inProgressSprite == null)      Debug.Log("InProgressSprite is NULL");
        if (_canCompleteSprite == null)     Debug.Log("CanCompleteSprite is NULL");

    }
    void OnDestroy()
    {
        QuestEvents.QuestCanCompleteEvent -= HandleQuestCanComplete;
    }
    void Start()
    {
        if(_detailPopup == null)            _detailPopup = FindFirstObjectByType<QuestDetailPopup>();
    }
    void RefreshUI()
    {
        _title.SetText($"{_data.QuestName}");
    }
    void RefreshStateIcon()
    {
        if (_data == null)
            return;

        if (QuestManager.Instance == null)
            return;

        if (!QuestManager.Instance.QuestRunTimeStateTable.TryGetValue(_data.Id, out QuestRunTimeState state))
            return;

        if (state.Status == QuestStatusType.CANCOMPLETE)
        {
            _icon.sprite = _canCompleteSprite;
        }
        else
        {
            _icon.sprite = _inProgressSprite;
        }
    }
    public void Init(QuestData questData)
    {
        _data = questData;
        RefreshUI();
        RefreshStateIcon();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(eventData.pointerEnter)
        {
            _detailPopup.RefreshUI(_data);
            _detailPopup.Show();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(eventData.pointerEnter)
        {
            _detailPopup.Hide();
        }
    }

    public void HandleQuestCanComplete(QuestData data)
    {
        if (_data.Id != data.Id) return;
        _icon.sprite = _canCompleteSprite;
    }
}
