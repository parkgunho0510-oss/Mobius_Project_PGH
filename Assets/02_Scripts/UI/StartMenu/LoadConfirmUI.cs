using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LoadConfirmUI : MonoBehaviour
{
    [SerializeField] GameObject _root;

    [Header("Load Popup Buttons")]
    [SerializeField] private Button _btnLoadOk;
    [SerializeField] private Button _btnLoadClose;
    void Awake()
    {
        if (_root == null) _root = transform.GetChild(0).gameObject;
        _btnLoadOk.onClick.AddListener(OnClickLoadOk);
        _btnLoadClose.onClick.AddListener(Hide);
        Hide();
    }
    void OnDestroy()
    {
        _btnLoadOk.onClick.RemoveListener(OnClickLoadOk);
        _btnLoadClose.onClick.RemoveListener(Hide);
    }
    void OnClickLoadOk()
    {
        Hide();
        var op = SceneManager.LoadSceneAsync("Lobby");

        op.completed += HandleSceneLoadedCompleted;

    }
    public void Hide()
    {
        _root.SetActive(false);
    }
    public void Show()
    {
        _root.SetActive(true);
    }

    void HandleSceneLoadedCompleted(AsyncOperation op)
    {
        SaveManager.Instance.LoadGame();
    }

}
