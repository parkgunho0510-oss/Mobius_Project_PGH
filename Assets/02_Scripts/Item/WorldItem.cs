
using UnityEngine;
using System.Collections.Generic;

public class WorldItem : MonoBehaviour
{
    [SerializeField] ItemData _data;
    [SerializeField] private int _amount;
    public ItemData Data {  get { return _data; } private set { } }
    public int Amount { get { return _amount; } private set { } }
    public void Init(ItemData data, int amount)
    {
        _data = data;
        _amount = amount;
    }
}
