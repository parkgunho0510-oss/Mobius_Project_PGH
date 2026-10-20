using UnityEngine;

public class MonsterDrop : MonoBehaviour
{
    [SerializeField] string _dropTableID = "";
    [SerializeField] float _dropRadius = 5f;
    [SerializeField] GameObject _defaultDropItem;

    public void Drop()
    {
        var dropList = DataManager.Instance.GetDropTable(_dropTableID);
        if (dropList == null) return;

        int count = dropList.Count;

        for(int i = 0; i < count; i++)
        {
            float rate = Random.Range(0f, 1f);
            if(rate <= dropList[i].DropRate)
            {
                Vector2 offset = Random.insideUnitCircle * _dropRadius;
                Vector3 pos = new Vector3(transform.position.x + offset.x, transform.position.y, transform.position.z + offset.y);
                GameObject item = Instantiate(_defaultDropItem, pos, Quaternion.identity);

                int amount = Random.Range(dropList[i].MinAmount, dropList[i].MaxAmount + 1);
                if(item.TryGetComponent(out WorldItem worldItem))
                {
                    worldItem.Init(dropList[i].ItemData, amount);
                }
                else
                {
                    item.AddComponent<WorldItem>().Init(dropList[i].ItemData, amount);
                }
            }
        }
    }
}
