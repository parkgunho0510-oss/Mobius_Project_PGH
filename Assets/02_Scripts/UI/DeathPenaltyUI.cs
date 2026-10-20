using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathPenaltyUI : MonoBehaviour
{
    [SerializeField] private GameObject _deathPenaltyUIroot;

    [SerializeField] private Inventory _inventory;
    [Header("Slot")]
    [SerializeField] private Transform _slotParent;
    [SerializeField] private DeathPenaltySlotUI _slotPrefab;

    [Header("Button")]
    [SerializeField] private Button _confirmButton;

    [SerializeField] private PenaltyConfirmPopup _confirmPopup;

    [Header("Select")]
    [SerializeField] private int _requiredSelectCount = 2;

    private InventoryItem[] _items;
    private List<DeathPenaltySlotUI> _slots = new();
    private List<int> _selectedIndexes = new();

    private System.Action _onComplete;

    private void Awake()
    {
        if (_confirmButton != null)
            _confirmButton.onClick.AddListener(OnClickConfirm);

        if (_deathPenaltyUIroot != null)
            _deathPenaltyUIroot.SetActive(false);

        if (_inventory == null)
        {
            _inventory = FindFirstObjectByType<Inventory>();
        }
    }
    public void Open(System.Action onComplete)
    {
        _onComplete = onComplete;

        _deathPenaltyUIroot.SetActive(true);

        _items = InventoryManager.Instance.Items;

        Debug.Log($"DeathPenaltyUI Open / items null? {_items == null}");

        if (_items != null)
            Debug.Log($"인벤토리 배열 길이 : {_items.Length}");

        _selectedIndexes.Clear();

        int itemCount = 0;

        foreach (InventoryItem item in _items)
        {
            if (item != null)
                itemCount++;
        }

        _requiredSelectCount = Mathf.Min(2, itemCount);

        GenerateSlots();
        RefreshConfirmButton();
    }

    private void GenerateSlots()
    {
        ClearSlots();

        if (_items == null)
        {
            Debug.LogError("_items가 null입니다.");
            return;
        }

        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] == null)
                continue;

            Debug.Log($"사망 패널티 슬롯 생성 대상 : {i} / {_items[i].Data.ItemName}");

            CreateSlot(i, _items[i]);
        }
    }

    private void CreateSlot(int index, InventoryItem item)
    {
        DeathPenaltySlotUI slot = Instantiate(_slotPrefab, _slotParent);

        _slots.Add(slot);

        ItemData data = item.Data;

        int amount = 1;

        if (item is ConsumableItem ci)
            amount = ci.Amount;

        slot.Init(index, data.Icon, amount, OnSelectSlot);
        slot.SetSelected(false);
    }

    private void OnSelectSlot(DeathPenaltySlotUI selectedSlot)
    {
        int index = selectedSlot.InventoryIndex;

        if (_selectedIndexes.Contains(index))
        {
            _selectedIndexes.Remove(index);
            selectedSlot.SetSelected(false);

            Debug.Log($"선택 해제 슬롯 : {index}");
        }
        else
        {
            if (_selectedIndexes.Count >= _requiredSelectCount)
            {
                Debug.Log($"이미 {_requiredSelectCount}개 선택됨");
                return;
            }

            _selectedIndexes.Add(index);
            selectedSlot.SetSelected(true);

            Debug.Log($"선택된 슬롯 : {index}");
        }

        RefreshConfirmButton();
    }

    private void RefreshConfirmButton()
    {
        if (_confirmButton != null)
            _confirmButton.interactable = _selectedIndexes.Count == _requiredSelectCount;
    }

    private void OnClickConfirm()
    {
        Debug.Log("확인 버튼 클릭");

        if (_selectedIndexes.Count != _requiredSelectCount)
        {
            Debug.Log($"선택된 슬롯 수 부족 : {_selectedIndexes.Count}");
            return;
        }

        if (_confirmPopup != null)
        {
            Debug.Log("컨펌 팝업 열기");
            _confirmPopup.Open(ApplyDeathPenalty);
        }
        else
        {
            Debug.LogError("_confirmPopup null");
        }
    }

    private void ClearSlots()
    {
        foreach (var slot in _slots)
        {
            Destroy(slot.gameObject);
        }

        _slots.Clear();
    }

    private void ApplyDeathPenalty()
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] == null)
                continue;

            if (_selectedIndexes.Contains(i))
                continue;

            ItemData itemData = _items[i].Data;

            if (itemData.Type == ItemType.CONSUMABLE || itemData.Type == ItemType.EQUIPMENT)
            {
                _items[i] = null;
            }
        }
        _inventory.TrimAll();
        _deathPenaltyUIroot.SetActive(false);

        _onComplete?.Invoke();
    }
}