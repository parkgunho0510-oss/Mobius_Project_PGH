using UnityEngine;
using UnityEngine.InputSystem;

public class ESCUIController : MonoBehaviour
{
    [Header("Player Input")]
    [SerializeField] private PlayerInput _playerInput;

    [Header("ESC UI")]
    [SerializeField] private GameObject _escPanel;

    [Header("Root Panels")]
    [SerializeField] private GameObject[] _rootPanels;

    [Header("Death Penalty UI")]
    [SerializeField] private GameObject _deathPenaltyPanel_root;

    private InputAction _playerEscapeAction;
    private InputAction _escEscapeAction;
    private InputAction _uiEscapeAction;

    private void Awake()
    {
        if (_playerInput == null)
            _playerInput = FindFirstObjectByType<PlayerInput>();

        _playerEscapeAction = _playerInput.actions.FindAction("Player/Escape");
        _escEscapeAction = _playerInput.actions.FindAction("ESC/Escape");
        _uiEscapeAction = _playerInput.actions.FindAction("UI/Escape");

        _playerEscapeAction.performed += OnPlayerEscape;
        _escEscapeAction.performed += OnESCEscape;
        _uiEscapeAction.performed += OnUIEscape;

        CloseRootPanelsOnly();
        _escPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_playerEscapeAction != null)
            _playerEscapeAction.performed -= OnPlayerEscape;

        if (_escEscapeAction != null)
            _escEscapeAction.performed -= OnESCEscape;

        if (_uiEscapeAction != null)
            _uiEscapeAction.performed -= OnUIEscape;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    private bool IsDeathPenaltyOpen()
    {
        return _deathPenaltyPanel_root != null && _deathPenaltyPanel_root.activeSelf;
    }

    private void OnPlayerEscape(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        OpenESCPanel();
    }

    private void OnUIEscape(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        // 사망 패널티 창이 켜져있는 상태라면 Player로 복귀하지 않고 ESC창만 연다
        if (IsDeathPenaltyOpen())
        {
            OpenESCPanel();
            return;
        }

        CloseAllPanels();
        ResumePlayer();
    }

    private void OnESCEscape(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        CloseESCPanel();
    }

    private void OpenESCPanel()
    {
        CloseRootPanelsOnly();

        _escPanel.SetActive(true);

        UIEvents.RaiseCursorShowed();
        Time.timeScale = 0f;

        _playerInput.SwitchCurrentActionMap("ESC");
    }

    public void CloseESCPanel()
    {
        CloseRootPanelsOnly();

        if (IsDeathPenaltyOpen())
        {
            // 사망 패널티가 남아있으면 UI 상태 유지
            UIEvents.RaiseCursorShowed();
            Time.timeScale = 0f;
            _playerInput.SwitchCurrentActionMap("UI");
        }
        else
        {
            ResumePlayer();
        }
    }

    public void CloseAllPanels()
    {
        CloseRootPanelsOnly();
        _escPanel.SetActive(false);
    }

    private void CloseRootPanelsOnly()
    {
        for (int i = 0; i < _rootPanels.Length; i++)
        {
            if (_rootPanels[i] != null)
                _rootPanels[i].SetActive(false);
        }
    }

    private void ResumePlayer()
    {
        Time.timeScale = 1f;

        UIEvents.RaiseCursorHide();

        _playerInput.SwitchCurrentActionMap("Player");
    }
}