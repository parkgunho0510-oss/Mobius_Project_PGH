using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TutorialNpc : NpcController
{
    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        SceneManager.sceneLoaded -= HandleSceneLoaded;
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
        UIEvents.RaiseCursorHide();
        UIEvents.RaiseUIClosed();
    }
    protected override void HandleFriendshipUpdated(string id, float value)
    {
        base.HandleFriendshipUpdated(id, value);
    }
    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Tutorial")
        {
            StartCoroutine(Co_LoadTutorialDialogue());
        }
    }

    IEnumerator Co_LoadTutorialDialogue()
    {
        yield return new WaitForSeconds(0.6f);
        if (_data.Profiles[(int)_runTimeState.NpcStateType].CanInteract)
        {
            UIEvents.RaiseUIOpened();
            UIEvents.RaiseCursorShowed();
            UIEvents.RaiseNpcInteracted(_data, _runTimeState);
            QuestEvents.RaiseTalkToNpc(_data);
        }
    }
}
