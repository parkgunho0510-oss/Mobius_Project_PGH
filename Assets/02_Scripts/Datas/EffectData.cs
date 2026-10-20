using UnityEngine;

[CreateAssetMenu(fileName = "EffectData", menuName = "Scriptable Objects/EffectData")]
public class EffectData : ScriptableObject, IDataEntity
{
    // ID (ex: EFF_BLOOD, EFF_SPARK)
    [SerializeField] private string id;
    public string ID => id;

    public EffectType Type;

    public GameObject Prefab;

    //  DefaultPoolSize
    public int DefaultPoolSize;

    public void LinkData(DataManager manager)
    {

    }
}
