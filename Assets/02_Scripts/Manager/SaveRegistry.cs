using System.Collections.Generic;

public static class SaveRegistry
{
    public static Dictionary<string, ISaveable> SaveableTable = new();
    public static IEnumerable<ISaveable> All => SaveableTable.Values;

    public static void Register(ISaveable saveable)
    {
        if (saveable == null) return;

        string id = saveable.SaveId;

        if (string.IsNullOrEmpty(id)) return;

        SaveableTable[id] = saveable;
    }

    public static void Unregister(ISaveable saveable)
    {
        if (saveable == null) return;

        string id = saveable.SaveId;

        if (string.IsNullOrEmpty(id)) return;

        if (!SaveableTable.TryGetValue(id, out ISaveable current))
            return;

        if (!ReferenceEquals(current, saveable))
            return;

        SaveableTable.Remove(id);
    }

    public static void Clear()
    {
        SaveableTable.Clear();
    }

    public static bool TryGet(string id, out ISaveable saveable)
    {
        return SaveableTable.TryGetValue(id, out saveable);
    }
}