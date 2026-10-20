using System.Collections.Generic;
using UnityEngine;

public class MonsterPooling : MonoBehaviour
{
    private Dictionary<string, ObjectPool<GameObject>> _poolTable = new();

    public void InitPool(string id, GameObject prefab, int count)
    {
        if (_poolTable.ContainsKey(id)) return;

        _poolTable[id] = new ObjectPool<GameObject>(count, () =>
        {
            GameObject obj = Instantiate(prefab);
            obj.transform.SetParent(this.transform);
            obj.SetActive(false);
            return obj;
        });
    }

    public GameObject Get(string id) => _poolTable.TryGetValue(id, out var pool) ? pool.Get() : null;

    public void Release(string id, GameObject obj)
    {
        if (_poolTable.ContainsKey(id))
        {
            obj.SetActive(false);
            _poolTable[id].Set(obj);
        }
    }
}
