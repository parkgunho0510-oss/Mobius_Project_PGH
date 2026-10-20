using UnityEngine;
using UnityEngine.UI;

public class QuestCompletePopup : MonoBehaviour
{
    [SerializeField] GameObject _root;
    [SerializeField] Button _completeBtn;
    [SerializeField] Button _cancelBtn;
    public bool IsVisible => _root.activeSelf;
    string _currentID = "";
    void Awake()
    {
        if (_root == null) _root = transform.GetChild(0).gameObject;
        if (_completeBtn == null) Debug.Log("Complete Btn is NULL");
        if (_cancelBtn == null) Debug.Log("Cancel Btn is NULL");

        if (_completeBtn != null) _completeBtn.onClick.AddListener(() => OnClickCompleteBtn());
        if (_cancelBtn != null) _cancelBtn.onClick.AddListener(() => OnClickCancelBtn());
    }
    void Start()
    {
        Hide();
    }
    void OnClickCompleteBtn()
    {
        QuestEvents.RaiseCompletedBtnClick(_currentID);
        Hide();
    }
    void OnClickCancelBtn()
    {
        _currentID = "";
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
}
