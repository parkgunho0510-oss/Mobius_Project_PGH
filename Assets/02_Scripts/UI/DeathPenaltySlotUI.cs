using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeathPenaltySlotUI : MonoBehaviour, IPointerClickHandler
{

    
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _amountText;
    [SerializeField] private GameObject _selectHighlight;

    public int InventoryIndex { get; private set; }

    private Action<DeathPenaltySlotUI> _onClick;

    public void Init(int inventoryIndex, Sprite icon, int amount, Action<DeathPenaltySlotUI> onClick)
    {
        InventoryIndex = inventoryIndex;
        _onClick = onClick;

        if (_iconImage != null)
        {
            _iconImage.sprite = icon;
            _iconImage.gameObject.SetActive(icon != null);
        }

        if (_amountText != null)
        {
            _amountText.text = amount.ToString();
            _amountText.gameObject.SetActive(amount > 1);
        }

        SetSelected(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _onClick?.Invoke(this);
    }

    public void SetSelected(bool selected)
    {
        if (_selectHighlight != null)
            _selectHighlight.SetActive(selected);
    }
}