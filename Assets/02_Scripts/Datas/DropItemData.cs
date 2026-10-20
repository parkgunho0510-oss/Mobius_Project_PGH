using UnityEngine;

[CreateAssetMenu(fileName = "DropItemData", menuName = "Scriptable Objects/DropItemData")]
public class DropItemData : ScriptableObject, IDataEntity
{
    // ID 
    [SerializeField] private string id;
    public string ID => id;

    //  TableID (Link to MonsterDB (DropTableID))
    // Ex: DRP_SKE_001 이라는 값과 같은 행들이 하나의 드랍 그룹이 됨
    public string TableID;

    // ItemData (Item - ID)
    public string ItemDataID;

    // 실질적인 아이템 데이터 (로드 후 ItemDataID로 검색해서 채워넣음)
    [HideInInspector] public ItemData ItemData;
    
    public float DropRate;
    public int MinAmount = 1; 
    public int MaxAmount = 1;

    public void LinkData(DataManager manager)
    {
        // DataManager에서 ItemDataID(Ex: POT_BAG_001)에 해당하는 실제 ItemData 객체 서칭
        // 찾은 객체를 itemData 변수에 할당
        if (!string.IsNullOrEmpty(ItemDataID))
        {
            ItemData = manager.GetItem(ItemDataID);
            
            if (ItemData == null)
            {
                Debug.LogWarning($"{ItemDataID}에 해당하는 아이템 데이터를 찾을 수 없습니다! (ID: {id})");
            }
        }
    }
}
