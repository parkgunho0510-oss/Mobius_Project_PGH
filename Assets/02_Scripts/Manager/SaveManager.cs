using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    [SerializeField] PlayerStatus _playerStatus;
    [SerializeField] Inventory _inventory;

    readonly Dictionary<string, NpcSaveData> _npcRuntimeCache = new();
    string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    public bool Exists => File.Exists(SavePath);

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }
    
    void ApplyNpcRuntimeCacheToCurrentScene()
    {
        foreach (NpcSaveData saveData in _npcRuntimeCache.Values)
        {
            if (!SaveRegistry.TryGet(saveData.NpcID, out ISaveable saveable))
            {
                Debug.LogWarning($"[NpcSave] 현재 씬에서 NPC를 못 찾음: {saveData.NpcID}");
                continue;
            }

            if (saveable is NpcRunTimeStateProxy npcProxy)
            {
                npcProxy.LoadSaveData(saveData);

                Debug.Log($"[NpcSave] Apply Load: {saveData.NpcID}, State: {saveData.NpcStateType}, Friendship: {saveData.Friendlyship}");
            }
        }
    }
    void Write(GameSaveData data)
    {
        data.SaveVersion = "1.0.0";
        data.SavedAt = DateTime.UtcNow.ToString("o");
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
#if UNITY_EDITOR
        Debug.Log($"[SaveSystem] Saved: {SavePath}\n{json}");
#endif
    }

    GameSaveData Read()
    {
        if(!Exists) return null;
        string json = File.ReadAllText(SavePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
        return data;
    }

    void CacheNpcSaveData(List<NpcSaveData> npcSaveDatas)
    {
        if (npcSaveDatas == null) return;

        foreach (NpcSaveData npcSaveData in npcSaveDatas)
        {
            if (string.IsNullOrEmpty(npcSaveData.NpcID))
                continue;

            _npcRuntimeCache[npcSaveData.NpcID] = npcSaveData;
        }
    }
    public void ResetForNewGame()
    {
        _npcRuntimeCache.Clear();

        _playerStatus = null;
        _inventory = null;

        SaveRegistry.Clear();

        Debug.Log("[SaveManager] New Game Runtime Cache Cleared");
    }
    public void SaveGame()
    {
        if (_playerStatus == null)
            _playerStatus = FindFirstObjectByType<PlayerStatus>();

        if (_inventory == null)
            _inventory = FindFirstObjectByType<Inventory>();

        CacheCurrentNpcRuntimeStates();

        GameSaveData data = new();
        data.IsFirstGame = false;

        data.InventorySaveData = _inventory.CreateSaveData();
        data.PlayerSaveData = _playerStatus.CreatePlayerSaveData();
        data.PlayerStatSaveData = _playerStatus.CreateStatSaveData();
        data.CurrentUsingItemSaveData = InventoryManager.Instance.CreateSaveData();
        data.QuestSaveDatas = QuestManager.Instance.CreateSaveData();

        foreach (NpcSaveData npcSaveData in _npcRuntimeCache.Values)
        {
            data.NpcSaveDatas.Add(npcSaveData);
        }

        Write(data);
    }
    public void LoadGame()
    {
        GameSaveData data = Read();

        if (data == null)
            return;

        _npcRuntimeCache.Clear();

        CacheNpcSaveData(data.NpcSaveDatas);

        if (_playerStatus == null)
            _playerStatus = FindFirstObjectByType<PlayerStatus>();

        if (_inventory == null)
            _inventory = FindFirstObjectByType<Inventory>();

        _inventory.LoadSaveData(data.InventorySaveData);
        _playerStatus.LoadPlayerSaveData(data.PlayerSaveData);
        _playerStatus.LoadStatSaveData(data.PlayerStatSaveData);
        QuestManager.Instance.LoadSaveData(data.QuestSaveDatas);
        InventoryManager.Instance.LoadSaveData(data.CurrentUsingItemSaveData);

        ApplyNpcRuntimeCacheToCurrentScene();
    }
    public void Delete()
    {
        if (Exists)
            File.Delete(SavePath);

        ResetForNewGame();

        if (QuestManager.Instance != null)
            QuestManager.Instance.ResetForNewGame();

        //if (InventoryManager.Instance != null)
        //    InventoryManager.Instance.ResetToDefaultData();

        if (StatManager.Instance != null)
            StatManager.Instance.InitStats();

        if (GoldManager.Instance != null)
            GoldManager.Instance.InitGold();

        Debug.Log("[SaveManager] Save File Deleted And Runtime Reset");
    }
    public bool CanLoadGame()
    {
        if (!Exists)
            return false;

        GameSaveData data = Read();

        if (data == null)
            return false;

        return !data.IsFirstGame;
    }
    public void CacheCurrentNpcRuntimeStates()
    {
        foreach (ISaveable saveable in SaveRegistry.All)
        {
            if (saveable is not NpcRunTimeStateProxy npcProxy)
                continue;

            NpcSaveData saveData = npcProxy.CreateSaveData();

            if (saveData == null)
                continue;

            if (string.IsNullOrEmpty(saveData.NpcID))
                continue;

            _npcRuntimeCache[saveData.NpcID] = saveData;

            Debug.Log($"[NpcSave] Cache Save: {saveData.NpcID}, State: {saveData.NpcStateType}, Friendship: {saveData.Friendlyship}");
        }
    }
    IEnumerator Co_ApplyNpcRuntimeCacheAfterSceneLoaded()
    {
        yield return null;

        ApplyNpcRuntimeCacheToCurrentScene();
    }
    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Init") return;
        if (scene.name == "StartScene_LCS") return;

        if (_playerStatus == null)
            _playerStatus = FindFirstObjectByType<PlayerStatus>();

        if (_inventory == null)
            _inventory = FindFirstObjectByType<Inventory>();

        if (_npcRuntimeCache.Count > 0)
        {
            StartCoroutine(Co_ApplyNpcRuntimeCacheAfterSceneLoaded());
        }
    }


}
