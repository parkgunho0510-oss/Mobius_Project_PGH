using UnityEngine;

[CreateAssetMenu(fileName = "NpcStateProfileData", menuName = "Scriptable Objects/NpcStateProfileData")]
public class NpcStateProfileData : ScriptableObject
{
    public NpcStateType StateType = NpcStateType.DEFAULT;
    public DialogueData DialogueData;
    public bool CanInteract = true;
    public bool HasQuest = false;
}
