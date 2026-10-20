using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/DialogueData")]
public class DialogueData : ScriptableObject
{
    public string DialogueID;
    public string StartNodeID;
    public List<DialogueNodeData> DialogueNodeDatas = new();
}
