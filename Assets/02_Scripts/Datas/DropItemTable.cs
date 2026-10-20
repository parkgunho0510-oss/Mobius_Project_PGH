using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "DropItemTable", menuName = "Scriptable Objects/DropItemTable")]
public class DropItemTable : ScriptableObject
{
    public List<DropItemData> DropItemDatas = new();
}
