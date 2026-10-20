using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PotionHandler : MonoBehaviour
{
    [SerializeField] private CurrentUsingItems _currentUsingItems;
    [SerializeField] private InventoryUI _inventoryUI;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private PlayerStatus _playerStatus;
    [SerializeField] private ItemQuickSlotUI _itemquickSlotUI;

    private InputAction _quickSlotItemAction1;
    private InputAction _quickSlotItemAction2;

    private bool _isCooldown1 = false;
    private bool _isCooldown2 = false;

    private float _cooldownTimer1 = 0f;
    private float _cooldownTimer2 = 0f;

    private float _coolTime1 = 0f;
    private float _coolTime2 = 0f;

    private void Awake()
    {
        _itemquickSlotUI = GetComponent<ItemQuickSlotUI>();
        _currentUsingItems = FindFirstObjectByType<CurrentUsingItems>();
        _inventoryUI = FindFirstObjectByType<InventoryUI>();
        _playerInput = FindFirstObjectByType<PlayerInput>();
        _playerStatus = FindFirstObjectByType<PlayerStatus>();
        _quickSlotItemAction1 = _playerInput.actions["QuickSlotItem1"];
        _quickSlotItemAction2 = _playerInput.actions["QuickSlotItem2"];

        _quickSlotItemAction1.performed += OnUsePotion1;
        _quickSlotItemAction2.performed += OnUsePotion2;
    }
    private void OnDestroy()
    {
        if (_quickSlotItemAction1 != null)
            _quickSlotItemAction1.performed -= OnUsePotion1;

        if (_quickSlotItemAction2 != null)
            _quickSlotItemAction2.performed -= OnUsePotion2;
    }

    private IEnumerator Start()
    {
        yield return null;
        yield return null;
        

        RefreshPotion();
    }

    private void Update()
    {
        UpdateCooldown();
    }

    private void OnUsePotion1(InputAction.CallbackContext callback)
    {
        if (!callback.performed)
            return;

        UsePotionBySlot(1);
    }

    private void OnUsePotion2(InputAction.CallbackContext callback)
    {
        if (!callback.performed)
            return;

        UsePotionBySlot(2);
    }

    private void UsePotionBySlot(int slotIndex)
    {
        Debug.Log($"PotionHandler 실행 / slot:{slotIndex} / object:{gameObject.name} / id:{GetInstanceID()}");
        if (_playerStatus.IsDead)
        {
            Debug.Log("플레이어 사망 상태라 포션 사용 불가");
            return;
        }

        Debug.Log($"{slotIndex}번키 입력");

        ConsumableData potionData = GetPotionData(slotIndex);

        if (potionData == null)
        {
            Debug.Log($"{slotIndex}번 슬롯 장착된 소비 아이템 없음");
            return;
        }

        string potionType = potionData.ID.Split('_')[1];

        if (potionType == "STA")
        {
            UseStaminaPotion(slotIndex);
        }
        else
        {
            UseHPPotion(slotIndex);
        }
    }

    private void UseHPPotion(int slotIndex)
    {
        ConsumableData potionData = GetPotionData(slotIndex);
        int potionAmount = GetPotionAmount(slotIndex);

        if (potionData == null)
        {
            Debug.Log("장착된 포션 없음");
            return;
        }

        if (IsCooldown(slotIndex))
        {
            Debug.Log($"{slotIndex}번 슬롯 쿨타임 중");
            return;
        }

        if (potionAmount <= 0)
        {
            Debug.Log("포션 없음");
            return;
        }

        Debug.Log($"사용할 포션: {potionData.ItemName}, 수량: {potionAmount}");

        float healAmount = potionData.Value;
        float coolTime = potionData.CoolTime;

        _inventoryUI.DecreaseUsingPotionAmount(slotIndex);

        _playerStatus.Heal(healAmount);

        SetCooldown(slotIndex, coolTime);

        RefreshPotionSlot(slotIndex);
    }

    private void UseStaminaPotion(int slotIndex)
    {
        ConsumableData potionData = GetPotionData(slotIndex);
        int potionAmount = GetPotionAmount(slotIndex);

        if (potionData == null)
        {
            Debug.Log("장착 스테미나 포션 없음");
            return;
        }

        if (IsCooldown(slotIndex))
        {
            Debug.Log($"{slotIndex}번 슬롯 쿨타임 중");
            return;
        }

        if (potionAmount <= 0)
        {
            Debug.Log("포션 수량 없음");
            return;
        }

        Debug.Log($"사용 스테미나 포션 : {potionData.ItemName}, 수량 : {potionAmount}");

        float staminaAmount = potionData.Value;
        float coolTime = potionData.CoolTime;

        _inventoryUI.DecreaseUsingPotionAmount(slotIndex);

        _playerStatus.HealStamina(staminaAmount);

        SetCooldown(slotIndex, coolTime);

        RefreshPotionSlot(slotIndex);
    }

    public void RefreshPotion()
    {
        RefreshPotionSlot(1);
        RefreshPotionSlot(2);
    }

    private void RefreshPotionSlot(int slotIndex)
    {
        ConsumableData potionData = GetPotionData(slotIndex);
        int potionAmount = GetPotionAmount(slotIndex);

        if (potionData != null && potionAmount > 0)
        {
            _itemquickSlotUI.SetItem(slotIndex, potionData.Icon);
            _itemquickSlotUI.SetCount(slotIndex, potionAmount);
        }
        else
        {
            _itemquickSlotUI.SetItem(slotIndex, null);
            _itemquickSlotUI.SetCount(slotIndex, 0);
        }
    }

    private ConsumableData GetPotionData(int slotIndex)
    {
        return slotIndex == 1
            ? _currentUsingItems.Potion1
            : _currentUsingItems.Potion2;
    }

    private int GetPotionAmount(int slotIndex)
    {
        return slotIndex == 1
            ? _currentUsingItems.PotionAmount1
            : _currentUsingItems.PotionAmount2;
    }

    private bool IsCooldown(int slotIndex)
    {
        return slotIndex == 1 ? _isCooldown1 : _isCooldown2;
    }

    private void SetCooldown(int slotIndex, float coolTime)
    {
        if (slotIndex == 1)
        {
            _isCooldown1 = true;
            _cooldownTimer1 = coolTime;
            _coolTime1 = coolTime;
        }
        else
        {
            _isCooldown2 = true;
            _cooldownTimer2 = coolTime;
            _coolTime2 = coolTime;
        }
    }

    private void UpdateCooldown()
    {
        UpdateCooldownSlot(1);
        UpdateCooldownSlot(2);
    }

    private void UpdateCooldownSlot(int slotIndex)
    {
        if (!IsCooldown(slotIndex))
            return;

        if (slotIndex == 1)
        {
            _cooldownTimer1 -= Time.deltaTime;

            if (_cooldownTimer1 <= 0f)
            {
                _cooldownTimer1 = 0f;
                _isCooldown1 = false;
                _itemquickSlotUI.ClearCooldown(1);
                return;
            }

            _itemquickSlotUI.SetCooldown(1, _cooldownTimer1, _coolTime1);
        }
        else
        {
            _cooldownTimer2 -= Time.deltaTime;

            if (_cooldownTimer2 <= 0f)
            {
                _cooldownTimer2 = 0f;
                _isCooldown2 = false;
                _itemquickSlotUI.ClearCooldown(2);
                return;
            }

            _itemquickSlotUI.SetCooldown(2, _cooldownTimer2, _coolTime2);
        }
    }
}