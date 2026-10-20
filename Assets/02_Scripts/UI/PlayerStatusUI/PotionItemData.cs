using UnityEngine;

[CreateAssetMenu(fileName = "potionItemData ", menuName = "Scriptable Objects/PotionItemData")]


public class PotionItemData : ScriptableObject
{
    [Header("체력 물약 기본 정보")]
    [SerializeField] private string _itemName;
    [SerializeField] private int _healAmount;
    [SerializeField] private int _defaultCount;
    [SerializeField] private float _coolTime;

    public string ItemName => _itemName;
    public int HealAmount => _healAmount;
    public int DefaultCount => _defaultCount;
    public float CoolTime => _coolTime;
}