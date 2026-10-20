using UnityEngine;

public class NpcController : MonoBehaviour, IInteractable
{
    [SerializeField] protected NpcRunTimeState _runTimeState;
    [SerializeField] protected NpcData _data;

    public NpcRunTimeState RunTimeState => _runTimeState;
    public NpcData NpcData => _data;
    protected virtual void Awake()
    {
        UIEvents.OnDialogueEndedEvent += HandleDialogueEnded;
        QuestEvents.ReputationUpdatedEvent += HandleFriendshipUpdated;

        InitializeRuntimeState();
    }
    protected virtual void OnDestroy()
    {
        UIEvents.OnDialogueEndedEvent -= HandleDialogueEnded;
        QuestEvents.ReputationUpdatedEvent -= HandleFriendshipUpdated;
    }
    public void InitializeRuntimeState()
    {
        if (_runTimeState == null)
            _runTimeState = new NpcRunTimeState();

        if (_data != null && string.IsNullOrEmpty(_runTimeState.NpcID))
            _runTimeState.NpcID = _data.NpcID;
    }
    public virtual bool Interact() { return false; }
    protected virtual void HandleDialogueEnded(string id) { }
    protected virtual void HandleFriendshipUpdated(string id, float value) { }
    
}
