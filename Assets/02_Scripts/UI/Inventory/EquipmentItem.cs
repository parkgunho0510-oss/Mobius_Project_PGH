using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentItem : InventoryItem
{
    public EquipmentData EquipmentData { get; private set; }

    public EquipmentItem(EquipmentData data) : base(data)
    {
        EquipmentData = data;
    }
}
