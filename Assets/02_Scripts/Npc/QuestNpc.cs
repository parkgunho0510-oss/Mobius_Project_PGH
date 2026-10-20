using UnityEngine;

public class QuestNpc : NpcController
{
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
    public override bool Interact()
    {
        if (_data.Profiles[(int)_runTimeState.NpcStateType].CanInteract)
        {
            UIEvents.RaiseNpcInteracted(_data, _runTimeState);
            QuestEvents.RaiseTalkToNpc(_data);
            return true;
        }
        return false;
    }
    protected override void HandleDialogueEnded(string id)
    {
        if (id == _data.NpcID)
        {
            QuestManager.Instance.OpenQuestUI(_data.NpcID);

            if (_runTimeState.NpcStateType == NpcStateType.FRIENDLY) return;
            else if (_runTimeState.NpcStateType == NpcStateType.DEFAULT) _runTimeState.NpcStateType = NpcStateType.MET;
        }
    }
    protected override void HandleFriendshipUpdated(string id, float value)
    {
        if (id != _data.NpcID) return;
        _runTimeState.Friendship += value;


        if (_runTimeState.NpcStateType != NpcStateType.FRIENDLY && _runTimeState.Friendship >= 90f)
        {
            _runTimeState.NpcStateType = NpcStateType.FRIENDLY;
        }
    }
}
