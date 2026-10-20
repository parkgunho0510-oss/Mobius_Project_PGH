using UnityEngine;

[CreateAssetMenu(fileName = "ShopEntryData", menuName = "Scriptable Objects/ShopEntryData")]
public class ShopEntryData : ScriptableObject, IDataEntity
{
    // CSV Header: ID (Ex: ENT_STAGE_001)
    [SerializeField] private string id;
    public string ID => id;

    // CSV Header: ShopId (Id Key)
    // 어떤 상점에 소속될 품목인지 결정
    public string ShopId;

    // CSV Header: ItemID (Item - ID)
    public string ItemID;

    // Reference to actual ItemData (Resolved via ItemID lookup after loading)
    [HideInInspector] public ItemData Item;

    public int Price;
    public int Stock;
    public bool OverrideBasePrice = true;
    public bool InfiniteStock = false;

    public NpcStateType NpcStateType;
    public void LinkData(DataManager manager)
    {
        // Linked ItemData (Reference)
        if (!string.IsNullOrEmpty(ItemID))
        {
            Item = manager.GetItem(ItemID);

            if (Item == null)
            {
                Debug.LogWarning($"상점 품목 오류: {ItemID} 아이템을 찾을 수 없습니다. (ID: {ID})");
            }
        }
    }
}
