using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[DefaultExecutionOrder(-100)]
public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    /// <summary>
    /// Data Storage using DataParser 
    /// </summary>
    public DataParser<MonsterDB> monsters = new();
    public DataParser<EquipmentData> weapon = new();
    public DataParser<StageData> stage = new();
    public DataParser<StageMonsterData> stageMonster = new();
    public DataParser<LevelData> level = new();
    public DataParser<EffectData> effect = new();
    public DataParser<DropItemData> dropItem = new();
    public DataParser<ConsumableData> consumable = new();
    public DataParser<ShopData> shop = new();
    public DataParser<ShopEntryData> shopEntry = new();
    public DataParser<QuestData> quest = new();
    public DataParser<QuestObjectiveData> questObjective = new();
    public DataParser<QuestRewardData> questReward = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        { 
            Destroy(gameObject);
        }  
        Instance = this;
        LoadAllData();
    }

    /// <summary>
    /// All Items(Consumable & Equipment) Searching Dict at runtime 
    /// </summary>
    private Dictionary<string, ItemData> _allItemTable = new();

    /// <summary>
    /// Triggers the data loading process for all registered tables.
    /// Switch between CSV and Binary depending on the environment.
    /// </summary>
    void LoadAllData()
    {
        // 실 서비스 환경에서는 Binary 로드를 우선 권장
        //monsters.LoadFromBinary("MonsterTable");

        // 개발 환경에서는 CSV 로드
        monsters.LoadFromCsv("MonsterTable");
        weapon.LoadFromCsv("EquipmentTable");
        stage.LoadFromCsv("StageTable");
        stageMonster.LoadFromCsv("StageMonsterTable");
        level.LoadFromCsv("LevelTable");
        effect.LoadFromCsv("EffectTable");
        dropItem.LoadFromCsv("DropItemTable");
        consumable.LoadFromCsv("ConsumableTable");
        shop.LoadFromCsv("ShopTable");
        shopEntry.LoadFromCsv("ShopEntryTable");
        quest.LoadFromCsv("QuestTable");
        questObjective.LoadFromCsv("QuestObjectiveTable");
        questReward.LoadFromCsv("QuestRewardTable");


        BuildTotalItemTable();
        InitializeAllLinks();

    }

    private void BuildTotalItemTable()
    {
        _allItemTable.Clear();

        // Add EquipmentData in _allItemTable
        foreach (var pair in weapon.Table) _allItemTable[pair.Key] = pair.Value;
        // Add ConsumableData in _allItemTable
        foreach (var pair in consumable.Table) _allItemTable[pair.Key] = pair.Value;
    }

    private void InitializeAllLinks()
    {
        // Link to All tableData(csv) by using Interface
        LinkTable(stage.Table);
        LinkTable(monsters.Table);
        LinkTable(weapon.Table);
        LinkTable(stageMonster.Table);
        LinkTable(level.Table);
        LinkTable(effect.Table);
        LinkTable(dropItem.Table);
        LinkTable(consumable.Table);
        LinkTable(shop.Table);
        LinkTable(shopEntry.Table);
        LinkTable(quest.Table);
        LinkTable(questObjective.Table);
        LinkTable(questReward.Table);
    }

    private void LinkTable<T>(Dictionary<string, T> table) where T : IDataEntity
    {
        foreach (var entity in table.Values)
        {
            entity.LinkData(this);
        }
    }

    
    /// <summary>
    /// [Editor Only] Context menu to bake CSV data into Binary files.
    /// Right-click the DataManager component in the Inspector to see this option.
    /// </summary>
    [ContextMenu("Bake All Tables to Binary")]
    public void BakeAll()
    {
        // 1. 최신 CSV 내용을 먼저 메모리에 로드 
        monsters.LoadFromCsv("MonsterTable");
        weapon.LoadFromCsv("EquipmentTable");
        stage.LoadFromCsv("StageTable");
        stageMonster.LoadFromCsv("StageMonsterTable");
        level.LoadFromCsv("LevelTable");
        effect.LoadFromCsv("EffectTable");
        dropItem.LoadFromCsv("DropItemTable");
        consumable.LoadFromCsv("ConsumableTable");
        shop.LoadFromCsv("ShopTable");
        shopEntry.LoadFromCsv("ShopEntryTable");
        quest.LoadFromCsv("QuestTable");
        questObjective.LoadFromCsv("QuestObjectiveTable");
        questReward.LoadFromCsv("QuestRewardTable");

        // 2. 로드된 데이터를 바이너리 파일로 저장 
        monsters.SaveToBinary("MonsterTable");
        weapon.SaveToBinary("EquipmentTable");
        stage.SaveToBinary("StageTable");
        stageMonster.SaveToBinary("StageMonsterTable");
        level.SaveToBinary("LevelTable");
        effect.SaveToBinary("EffectTable");
        dropItem.SaveToBinary("DropItemTable");
        consumable.SaveToBinary("ConsumableTable");
        shop.SaveToBinary("ShopTable");
        shopEntry.SaveToBinary("ShopEntryTable");
        quest.SaveToBinary("QuestTable");
        questObjective.SaveToBinary("QuestObjectiveTable");
        questReward.SaveToBinary("QuestRewardTable");

        Debug.Log("<color=yellow>모든 테이블 바이너리 베이크 완료!</color>");
    }



    // 몬스터의 DropTableID를 받아 해당 그룹의 드롭 리스트를 반환
    public List<DropItemData> GetDropTable(string tableId)
    {
        return dropItem.Table.Values
            .Where(data => data.TableID == tableId)
            .ToList();
    }

    // 특정 상점의 모든 품목을 리스트로 반환
    public List<ShopEntryData> GetShopEntries(string shopId)
    {
        Debug.Log($"ShopData LinkData 호출됨 : {shopId}");
        return shopEntry.Table.Values
            .Where(data => data.ShopId == shopId)
            .ToList();
    }

    // QuestID 가 일치하는 퀘스트 목표 데이터들만 리스트로 반환
    public List<QuestObjectiveData> GetQuestObjectives(string questId)
    {
        return questObjective.Table.Values
            .Where(data => data.QuestID ==  questId)
            .ToList();

    }
    // QuestID 가 일치하는 퀘스트 보상 데이터들만 리스트로 반환
    public List<QuestRewardData> GetQuestRewards(string questId)
    {
        return questReward.Table.Values
            .Where(data => data.QuestID == questId)
            .ToList();

    }

    /// <summary>
    /// Retrieves monster data by its unique ID.
    /// </summary>
    /// <param name="id">The MonsterID defined in the Excel sheet.</param>
    /// <returns>MonsterData object if found, otherwise null.</returns>

    // ID로 무기/소모품 구분 없이 아이템 데이터를 가져오는 함수
    public ItemData GetItem(string id) => _allItemTable.GetValueOrDefault(id);

    // 외부(플레이어, 몬스터, 스포너 등)에서 데이터를 가져갈 때 쓰는 함수

    public EquipmentData GetWeapon(string id) => weapon.Table.GetValueOrDefault(id);
    public MonsterDB GetMonster(string id) => monsters.Table.GetValueOrDefault(id);
    public StageData GetStage(string id) => stage.Table.GetValueOrDefault(id);
    public StageMonsterData GetStageMonster(string id) => stageMonster.Table.GetValueOrDefault(id);
    public LevelData GetLevel(string id) => level.Table.GetValueOrDefault(id);
    public EffectData GetEffect(string id) => effect.Table.GetValueOrDefault(id);
    public DropItemData GetDropItem(string id) => dropItem.Table.GetValueOrDefault(id);
    public ConsumableData GetConsumableItem(string id) => consumable.Table.GetValueOrDefault(id);
    public ShopData GetShop(string id) => shop.Table.GetValueOrDefault(id);
    public QuestData GetQuest(string id) => quest.Table.GetValueOrDefault(id);
    
}