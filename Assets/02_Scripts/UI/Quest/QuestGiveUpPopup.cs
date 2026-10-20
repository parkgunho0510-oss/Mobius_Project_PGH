using UnityEngine;
using UnityEngine.UI;

public class QuestGiveUpPopup : MonoBehaviour
{
    [SerializeField] GameObject _root;
    [SerializeField] Button _giveUpBtn;
    [SerializeField] Button _cancelBtn;
    public bool IsVisible => _root.activeSelf;
    string _currentID = "";
    void Awake()
    {
        if (_root == null) _root = transform.GetChild(0).gameObject;
        if (_giveUpBtn == null) Debug.Log("GiveUp Btn is NULL");
        if (_cancelBtn == null) Debug.Log("Cancel Btn is NULL");

        if (_giveUpBtn != null) _giveUpBtn.onClick.AddListener(() => OnClickGiveUpBtn());
        if (_cancelBtn != null) _cancelBtn.onClick.AddListener(() => OnClickCancelBtn());
    }
    void Start()
    {
        Hide();
    }
    public void Show(string id)
    {
        _currentID = id;
        _root.SetActive(true);
    }
    public void Hide()
    {
        _root.SetActive(false);
    }
    void OnClickGiveUpBtn()
    {
        QuestEvents.RaiseGiveUpBtnClick(_currentID);
        Hide();
    }
    void OnClickCancelBtn()
    {
        _currentID = "";
        Hide();
    }

}
