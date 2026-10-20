using UnityEngine;

public class NpcRunTimeStateProxy : MonoBehaviour, INpcSaveable
{
    NpcController _npc;
    NpcRunTimeState _state;
    NpcData _data;

    public NpcRunTimeState NpcRunTimeState
    {
        get
        {
            EnsureInitialized();
            return _state;
        }
    }

    public NpcData NpcData
    {
        get
        {
            EnsureInitialized();
            return _data;
        }
    }

    public string SaveId
    {
        get
        {
            EnsureInitialized();
            return _data != null ? _data.NpcID : string.Empty;
        }
    }

    void Awake()
    {
        EnsureInitialized();
    }

    void OnEnable()
    {
        EnsureInitialized();

        if (string.IsNullOrEmpty(SaveId))
        {
            Debug.LogWarning($"[NpcSave] SaveId가 비어 있어서 등록 실패: {name}");
            return;
        }

        SaveRegistry.Register(this);
    }

    void OnDisable()
    {
        SaveRegistry.Unregister(this);
    }

    void EnsureInitialized()
    {
        if (_npc == null)
            _npc = GetComponent<NpcController>();

        if (_npc == null)
        {
            Debug.LogWarning($"[NpcSave] NpcController가 없음: {name}");
            return;
        }
        _npc.InitializeRuntimeState();
        _state = _npc.RunTimeState;
        _data = _npc.NpcData;
    }

    public NpcSaveData CreateSaveData()
    {
        EnsureInitialized();

        if (_data == null)
        {
            Debug.LogWarning($"[NpcSave] NpcData가 없어서 저장 실패: {name}");
            return null;
        }

        if (_state == null)
        {
            Debug.LogWarning($"[NpcSave] NpcRunTimeState가 없어서 저장 실패: {name}");
            return null;
        }

        NpcSaveData npcSaveData = new();
        npcSaveData.NpcID = _data.NpcID;
        npcSaveData.NpcStateType = _state.NpcStateType;
        npcSaveData.Friendlyship = _state.Friendship;

        return npcSaveData;
    }

    public void LoadSaveData(NpcSaveData data)
    {
        EnsureInitialized();

        if (data == null) return;
        if (_data == null) return;
        if (_state == null) return;
        if (data.NpcID != _data.NpcID) return;

        _state.NpcStateType = data.NpcStateType;
        _state.Friendship = data.Friendlyship;

        Debug.Log($"[NpcSave] NPC Load 완료: {data.NpcID}, State: {data.NpcStateType}, Friendship: {data.Friendlyship}");
    }
}