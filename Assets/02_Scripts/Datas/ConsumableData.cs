using UnityEngine;


[CreateAssetMenu(fileName = " ConsumableData", menuName = "Scriptable Objects/ConsumableData")]
public class ConsumableData : ItemData
{
    [Header("Consumable Specific")]
    public float Value;
    public float CoolTime;

    public override InventoryItem CreateItem()
    {
        return new ConsumableItem(this);
    }
}
