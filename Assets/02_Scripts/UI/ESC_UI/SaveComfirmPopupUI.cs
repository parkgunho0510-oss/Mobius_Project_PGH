using UnityEngine;
using UnityEngine.UI;

public class SaveConfirmPopupUI : MonoBehaviour
{
    [SerializeField] private GameObject _root;

    [Header("Button")]
    [SerializeField] private Button _btnOk;
    [SerializeField] private Button _btnCancel;

    private void Awake()
    {
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
        SaveManager.Instance.SaveGame();

        Debug.Log("Save Game");

        Close();
    }

    private void OnClickCancel()
    {
        Close();
    }
}