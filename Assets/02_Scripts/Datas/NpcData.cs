using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NpcData", menuName = "Scriptable Objects/NpcData")]
public class NpcData : ScriptableObject
{
    public string NpcID;
    public string NpcName;
    public Sprite Portrait;
    public List<NpcStateProfileData> Profiles = new();
}
