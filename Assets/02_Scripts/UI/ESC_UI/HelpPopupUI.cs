using UnityEngine;
using UnityEngine.UI;

public class HelpPopupUI : MonoBehaviour
{
    [Header("Close")]
    [SerializeField] private Button _closeButton;

    [Header("Root")]
    [SerializeField] private GameObject _rootPanel;

    [Header("Tab Buttons")]
    [SerializeField] private Button _controlButton;
    [SerializeField] private Button _storyButton;
    [SerializeField] private Button _infoButton;

    [Header("Contents")]
    [SerializeField] private GameObject _controlContent;
    [SerializeField] private GameObject _storyContent;
    [SerializeField] private GameObject _infoContent;

    private void Awake()
    {
        _controlButton.onClick.AddListener(ShowControlContent);
        _storyButton.onClick.AddListener(ShowStoryContent);
        _infoButton.onClick.AddListener(ShowInfoContent);

        if (_closeButton != null)
            _closeButton.onClick.AddListener(ClosePopup);
    }

    private void OnEnable()
    {
        ShowControlContent();
    }

    public void ShowControlContent()
    {
        SetContentActive(_controlContent);
    }

    public void ShowStoryContent()
    {
        SetContentActive(_storyContent);
    }

    public void ShowInfoContent()
    {
        SetContentActive(_infoContent);
    }

    public void SetContentActive(GameObject content)
    {
        _controlContent.SetActive(content == _controlContent);
        _storyContent.SetActive(content == _storyContent);
        _infoContent.SetActive(content == _infoContent);
    }

    public void ClosePopup()
    {
        if (_rootPanel != null)
            _rootPanel.SetActive(false);
    }
}