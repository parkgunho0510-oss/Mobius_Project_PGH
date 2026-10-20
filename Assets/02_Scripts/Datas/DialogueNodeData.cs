using UnityEngine;

[CreateAssetMenu(fileName = "DialogueNodeData", menuName = "Scriptable Objects/DialogueNodeData")]
public class DialogueNodeData : ScriptableObject
{
    public string NodeID;

    public string NpcName;
    public Sprite Portrait;

    [TextArea]
    public string Text;

    public string NextNodeID;
    public bool IsEnd;
}
