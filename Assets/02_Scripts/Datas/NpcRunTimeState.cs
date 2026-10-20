using System;
using UnityEngine;

[Serializable]
public class NpcRunTimeState
{
    public string NpcID;
    public NpcStateType NpcStateType;
    [Range(0f, 100f)]
    public float Friendship;
}
