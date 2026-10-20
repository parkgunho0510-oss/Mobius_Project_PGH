using UnityEngine;
using System.Collections.Generic;
public class EffectManager : MonoBehaviour
{
    [SerializeField] List<string> _effectIDList = new();
    Dictionary<EffectType, ObjectPool<GameObject>> _poolTable = new();

    void Awake()
    {
        
        EffectEvents.EffectPlayedEvent += HandleEffectPlayed;
        EffectEvents.EffectReturnedEvent += HandleEffectReturned;
    }
    void Start()
    {
        InitPool();
    }
    void OnDestroy()
    {
        EffectEvents.EffectReturnedEvent -= HandleEffectReturned;
        EffectEvents.EffectPlayedEvent   -= HandleEffectPlayed;
    }
    void InitPool()
    {
        foreach (string id in _effectIDList)
        {
            EffectData data = DataManager.Instance.GetEffect(id);
            _poolTable[data.Type] = new ObjectPool<GameObject>(data.DefaultPoolSize, () =>
            {
                GameObject pref = Instantiate(data.Prefab);
                pref.transform.parent = transform;
                pref.GetComponent<VfxAutoDestroy>().Type = data.Type;
                pref.SetActive(false);
                return pref;
            });
        }
    }
    void HandleEffectPlayed(EffectType type, Vector3 position, Quaternion rotation)
    {
        if(type == EffectType.NONE || type == EffectType.MAX)
        {
            Debug.Log("Type Error!");
            return;
        }

        GameObject vfx = _poolTable[type].Get();
        vfx.transform.position = position;
        vfx.transform.rotation = rotation;
        vfx.SetActive(true);
        vfx.GetComponent<VfxAutoDestroy>().Play();
    }
    void HandleEffectReturned(EffectType type, GameObject obj)
    {
        if (type == EffectType.NONE || type == EffectType.MAX)
        {
            Debug.Log("Type Error!");
            return;
        }
        obj.SetActive(false);
        _poolTable[type].Set(obj);
    }
}
