public interface INpcSaveable : ISaveable
{
    NpcSaveData CreateSaveData();
    void LoadSaveData(NpcSaveData data);
}