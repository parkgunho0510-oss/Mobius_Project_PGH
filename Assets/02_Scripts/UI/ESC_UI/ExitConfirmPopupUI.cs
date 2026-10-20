using UnityEngine;
using UnityEngine.UI;

public class ExitConfirmPopupUI : MonoBehaviour
{
    [SerializeField] private GameObject _root;

    [Header("Button")]
    [SerializeField] private Button _btnOk;
    [SerializeField] private Button _btnCancel;

    [Header("ESC UI")]
    [SerializeField] private ESCUI _escUI;

    private void Awake()
    {
       
        if (_escUI == null)
            _escUI = FindFirstObjectByType<ESCUI>();

        if (_root == null)
            _root = gameObject;

        _btnOk.onClick.AddListener(OnClickOk);
        _btnCancel.onClick.AddListener(OnClickCancel);

        Close();
    }

    public void Open()
    {
        _root.SetActive(true);
    }

    public void Close()
    {
        _root.SetActive(false);
    }

    private void OnClickOk()
    {
        if (_escUI != null)
            _escUI.OnClickExitToStart();

        Close();
    }

    private void OnClickCancel()
    {
        Close();
    }
}