using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanNotLoadConfirmPopupUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject _root;

    [Header("Buttons")]
    [SerializeField] private Button _okButton;
    [SerializeField] private Button _cancelButton;

    private void Awake()
    {
        _okButton.onClick.AddListener(OnClickYes);
        _cancelButton.onClick.AddListener(OnClickCancel);

        Hide();
    }

    public void Show()
    {
        _root.SetActive(true);
    }

    public void Hide()
    {
        _root.SetActive(false);
    }

    private void OnClickYes()
    {
        Hide();

        //SceneLoadContext.NextSceneName = "Tutorial";

        //SceneManager.LoadScene("Init");
        SceneManager.LoadScene("Tutorial");
    }

    private void OnClickCancel()
    {
        Hide();
    }
}