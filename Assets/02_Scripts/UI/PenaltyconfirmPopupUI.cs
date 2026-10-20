using System;
using UnityEngine;
using UnityEngine.UI;

public class PenaltyConfirmPopup : MonoBehaviour
{
    [SerializeField] private GameObject _penaltyConfirmPopuproot;
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    private Action _onConfirm;

    private void Awake()
    {
        if (_yesButton != null)
            _yesButton.onClick.AddListener(OnClickYes);

        if (_noButton != null)
            _noButton.onClick.AddListener(OnClickNo);

        _penaltyConfirmPopuproot.SetActive(false);
    }

    public void Open(Action onConfirm)
    {
        _onConfirm = onConfirm;

        _penaltyConfirmPopuproot.SetActive(true);
    }

    private void OnClickYes()
    {
        _onConfirm?.Invoke();

        _penaltyConfirmPopuproot.SetActive(false);
    }

    private void OnClickNo()
    {
        _penaltyConfirmPopuproot.SetActive(false);
    }
}