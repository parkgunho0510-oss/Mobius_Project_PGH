using UnityEngine;
using UnityEngine.UI;

public class QuestAcceptPopup : MonoBehaviour
{
    [SerializeField] GameObject _root;
    [SerializeField] Button _acceptBtn;
    [SerializeField] Button _cancelBtn;

    public bool IsVisible => _root.activeSelf;

    string _currentID = "";
    void Awake()
    {
        if (_root == null) _root = transform.GetChild(0).gameObject;
        if (_acceptBtn == null) Debug.Log("Accept Btn is NULL");
        if (_cancelBtn == null) Debug.Log("Cancel Btn is NULL");

        if(_acceptBtn != null) _acceptBtn.onClick.AddListener(()=>OnClickAcceptBtn());
        if(_cancelBtn != null) _cancelBtn.onClick.AddListener(()=>OnClickCancelBtn());

    }
    void Start()
    {
        Hide();
    }
    void OnClickAcceptBtn()
    {
        QuestEvents.RaiseAcceptedBtnClick(_currentID);
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
