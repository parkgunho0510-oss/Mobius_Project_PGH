using UnityEngine;
using UnityEngine.UI;
public class ExitConfirmUI : MonoBehaviour
{
    [SerializeField] GameObject _root;
    [Header("Exit Popup Buttons")]
    [SerializeField] private Button _btnExitOk;
    [SerializeField] private Button _btnExitClose;
    void Awake()
    {
        if (_root == null) _root = transform.GetChild(0).gameObject;
        _btnExitOk.onClick.AddListener(OnClickExitOk);
        _btnExitClose.onClick.AddListener(Hide);
        Hide();
    }
    void OnDestroy()
    {
        _btnExitOk.onClick.RemoveListener(OnClickExitOk);
        _btnExitClose.onClick.RemoveListener(Hide);
    }
    void OnClickExitOk()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void Hide()
    {
        _root.SetActive(false);
    }
    public void Show()
    {
        _root.SetActive(true);
    }
}
