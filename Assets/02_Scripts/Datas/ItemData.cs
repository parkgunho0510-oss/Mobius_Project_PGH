using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject, IDataEntity
{
    [Header("Base Info")]
    public string Id; // IDataEntity Override
    public string ID => Id;

    public string ItemName;
    public Sprite Icon;

    public ItemType Type;
    public int MaxStack = 1;
    public int BasePrice = 0;
    [TextArea] public string Description;

    [Header("World Presentation")]

    public GameObject WorldPrefab;

    public virtual InventoryItem CreateItem() { return null; }
    public virtual void LinkData(DataManager manager) { }
    
}
