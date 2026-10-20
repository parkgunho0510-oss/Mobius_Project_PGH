using System;
using System.Collections.Generic;
[Serializable]
public class GameSaveData
{
    public string SaveVersion;
    public string SavedAt;

    public PlayerSaveData PlayerSaveData;
    public PlayerStatSaveData PlayerStatSaveData;
    public InventorySaveData InventorySaveData;
    public CurrentUsingItemSaveData CurrentUsingItemSaveData;

    public List<QuestSaveData> QuestSaveDatas = new();
    public List<NpcSaveData> NpcSaveDatas = new();

    public bool IsFirstGame = true;
}
