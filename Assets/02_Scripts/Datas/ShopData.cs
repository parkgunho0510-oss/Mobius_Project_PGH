using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopData", menuName = "Scriptable Objects/ShopData")]
public class ShopData : ScriptableObject, IDataEntity
{
    // CSV Header : ID
    [SerializeField] private string id;
    public string ID => id;

    public string ShopId;

    // CSV Header: Description
    [TextArea] public string Description;

    /// <summary>
    /// List of items populated by DataManager using ShopId.
    /// </summary>
    [HideInInspector] public List<ShopEntryData> Entries = new();

    public void LinkData(DataManager manager)
    {
        // Loaded by DataManager: Searches and stores all sale items matching this ShopId
        Entries = manager.GetShopEntries(ShopId);
    }
}
