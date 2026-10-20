using UnityEngine;
using UnityEngine.UI;

public class SettingPopupUI : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private ConfirmPopupUI _confirmPopupUI;

    [Header("Tabs")]
    [SerializeField] private DisplayTabUI _displayTabUI;

    [Header("Close")]
    [SerializeField] private Button _closeButton;

    [Header("Root")]
    [SerializeField] private GameObject _rootPanel;

    [SerializeField] private GameObject _confirmPopupPanel;

    [Header("TabButtons")]
    [SerializeField] private Button _soundButton;
    [SerializeField] private Button _mouseButton;
    [SerializeField] private Button _displayButton;
    [SerializeField] private Button _deleteDataButton;

    [Header("Contents")]
    [SerializeField] private GameObject _soundContent;
    [SerializeField] private GameObject _mouseContent;
    [SerializeField] private GameObject _displayContent;
    [SerializeField] private GameObject _deleteDataContent;

    private void Awake()
    {
        _soundButton.onClick.AddListener(ShowSoundContent);
        _mouseButton.onClick.AddListener(ShowMouseContent);
        _displayButton.onClick.AddListener(ShowDisplayContent);
        _deleteDataButton.onClick.AddListener(ShowDeleteDataContent);

        if (_closeButton != null)
            _closeButton.onClick.AddListener(TryClosePopup);
    }

    private void OnEnable()
    {
        ShowDisplayContent();
    }

    private void OnDisable()
    {
        Debug.Log("SettingPopupUI OnDisable 실행됨");

        if (_confirmPopupUI == null)
        {
            Debug.LogError("ConfirmPopupUI 참조가 NULL");
            return;
        }

        _confirmPopupUI.Close();
    }
    void Start()
    {
        var soundTab = _soundContent.GetComponent<SoundTabUI>();
        soundTab.Init();
    }
    private void Update()
    {
        if (_rootPanel != null && !_rootPanel.activeSelf)
        {
            if (_displayTabUI != null && _displayTabUI.HasPendingChanges())
            {
                _displayTabUI.CancelDisplaySetting();
            }

            if (_confirmPopupPanel != null && _confirmPopupPanel.activeSelf)
            {
                _confirmPopupPanel.SetActive(false);
            }
        }
    }
    public void ShowSoundContent()
    {
        SetContentActive(_soundContent);
    }

    public void ShowMouseContent()
    {
        SetContentActive(_mouseContent);
    }

    public void ShowDisplayContent()
    {
        SetContentActive(_displayContent);
    }
    public void ShowDeleteDataContent()
    {
        SetContentActive(_deleteDataContent);
    }

    public void SetContentActive(GameObject activeContent)
    {
        _soundContent.SetActive(activeContent == _soundContent);
        _mouseContent.SetActive(activeContent == _mouseContent);
        _displayContent.SetActive(activeContent == _displayContent);
        _deleteDataContent.SetActive(activeContent == _deleteDataContent);
    }

    public void TryClosePopup()
    {
        if (_displayTabUI != null && _displayTabUI.HasPendingChanges())
        {
            if (_confirmPopupUI != null)
            {
                _confirmPopupUI.Open(
                    onApply: () =>
                    {
                        _displayTabUI.ApplyDisplaySetting();

                        if (_rootPanel != null)
                            _rootPanel.SetActive(false);
                    },
                    onDiscard: () =>
                    {
                        _displayTabUI.CancelDisplaySetting();

                        if (_rootPanel != null)
                            _rootPanel.SetActive(false);
                    }
                );
            }
        }
        else
        {
            if (_rootPanel != null)
                _rootPanel.SetActive(false);
        }
    }
}