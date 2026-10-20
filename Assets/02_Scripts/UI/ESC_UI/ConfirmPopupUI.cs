using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopupUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _panel;

    [Header("Buttons")]
    [SerializeField] private Button _applyButton;
    [SerializeField] private Button _discardButton;

    private Action _onApply;
    private Action _onDiscard;

    private void Awake()
    {
        _applyButton.onClick.AddListener(OnClickApply);
        _discardButton.onClick.AddListener(OnClickDiscard);

        Close();
    }
    


    public void Open(Action onApply, Action onDiscard)
    {
        _onApply = onApply;
        _onDiscard = onDiscard;

        _panel.SetActive(true);
        _panel.transform.SetAsLastSibling();
    }

    public void Close()
    {
        _panel.SetActive(false);
    }

    private void OnClickApply()
    {
        _onApply?.Invoke();

        ClearCallbacks();
        Close();
    }

    private void OnClickDiscard()
    {
        _onDiscard?.Invoke();

        ClearCallbacks();
        Close();
    }

    private void ClearCallbacks()
    {
        _onApply = null;
        _onDiscard = null;
    }
}