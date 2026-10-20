using UnityEngine;
using UnityEngine.UI;

public class CanNotSavePopupUI : MonoBehaviour
{
    [SerializeField] private GameObject _root;

    [Header("Button")]
    [SerializeField] private Button _closeBtn;

    private void Awake()
    {
        if (_root == null)
            _root = gameObject;

        _closeBtn.onClick.AddListener(OnClickOk);

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
        Close();
    }
}