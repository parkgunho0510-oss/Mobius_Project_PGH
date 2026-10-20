using UnityEngine;

/// <summary> 소비 아이템 (수량 관리 포함) </summary>
public class ConsumableItem : InventoryItem
{
    public ConsumableData ConsumableData { get; private set; }

    public int Amount { get; private set; }

    public int MaxAmount => ConsumableData.MaxStack;

    public bool IsMax => Amount >= MaxAmount;
    public bool IsEmpty => Amount <= 0;

    public ConsumableItem(ConsumableData data, int amount = 1) : base(data)
    {
        ConsumableData = data;
        SetAmount(amount);
    }

    public void SetAmount(int amount)
    {
        Amount = Mathf.Clamp(amount, 0, MaxAmount);
    }

    public int AddAmountAndGetExcess(int amount)
    {
        int nextAmount = Amount + amount;
        SetAmount(nextAmount);

        return (nextAmount > MaxAmount) ? (nextAmount - MaxAmount) : 0;
    }

    public ConsumableItem SeparateAndClone(int amount)
    {
        if (Amount <= 1) return null;

        if (amount > Amount - 1)
            amount = Amount - 1;

        Amount -= amount;
        return Clone(amount);
    }

    private ConsumableItem Clone(int amount)
    {
        return new ConsumableItem(ConsumableData, amount);
    }
}