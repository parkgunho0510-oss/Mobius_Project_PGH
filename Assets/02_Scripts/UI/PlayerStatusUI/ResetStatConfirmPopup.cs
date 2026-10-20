using UnityEngine;
using UnityEngine.UI;

public class ResetStatConfirmPopup : MonoBehaviour
{
    [SerializeField] private PlayerStatus _playerStatus;
    [SerializeField] Button _okBtn;
    [SerializeField] Button _cancelBtn;
    
    private void Awake()
    {
        if (_playerStatus == null) _playerStatus = FindFirstObjectByType<PlayerStatus>();

        if (_okBtn == null) Debug.Log("Ok Btn Is Null");
        if (_cancelBtn == null) Debug.Log("Cancel Btn Is Null");

        _okBtn.onClick.AddListener(() => OnClickConfirm());
        _cancelBtn.onClick.AddListener(() => OnClickCancel());
        
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        _okBtn.onClick.RemoveAllListeners();
        _cancelBtn.onClick.RemoveAllListeners();
    }
    void OnClickConfirm()
    {
        _playerStatus.ResetStat();
        Close();
    }

    void OnClickCancel()
    {
        Close();
    }


    public void Open()
    {
        Debug.Log("Open");
        gameObject.SetActive(true);
    }


    public void Close()
    {
        gameObject.SetActive(false);
    }
}
