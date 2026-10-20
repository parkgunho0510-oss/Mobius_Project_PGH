using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public InventoryItem[] Items { get; private set; }
    [SerializeField] private InventoryUI _inventoryUI;
    [SerializeField] private PotionHandler _potionHandler;
    [SerializeField] private int _maxCapacity = 64;
    [SerializeField] private int _initialCapacity = 32;

    public int Capacity { get; private set; }

    public bool HasInitializedUsingItems { get; set; }

    public ConsumableData Potion1 { get; set; }
    public ConsumableData Potion2 { get; set; }
    public EquipmentData Weapon1 { get; set; }
    public EquipmentData Weapon2 { get; set; }
    public EquipmentData Armor1 { get; set; }
    public EquipmentData Armor2 { get; set; }
    public int PotionAmount1 { get; set; }
    public int PotionAmount2 { get; set; }
    public EquipmentData CurrentWeapon { get; set; }

   

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Items = new InventoryItem[_maxCapacity];
        Capacity = _initialCapacity;

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        
    }
    public CurrentUsingItemSaveData CreateSaveData()
    {
        CurrentUsingItemSaveData saveData = new();

        if (Potion1 != null) { saveData.Potion1ID = Potion1.Id; saveData.PotionAmount1 = PotionAmount1; }
        else { saveData.Potion1ID = ""; saveData.PotionAmount1 = 0; }

        if (Potion2 != null) { saveData.Potion2ID = Potion2.Id; saveData.PotionAmount2 = PotionAmount2; }
        else { saveData.Potion2ID = ""; saveData.PotionAmount2 = 0; }

        if (Weapon1 != null) saveData.Weapon1ID = Weapon1.Id;
        else saveData.Weapon1ID = "";

        if (Weapon2 != null) saveData.Weapon2ID = Weapon2.Id;
        else saveData.Weapon2ID = "";

        if (Armor1 != null) saveData.Armor1ID = Armor1.Id;
        else saveData.Armor1ID = "";

        if (Armor2 != null) saveData.Armor2ID = Armor2.Id;
        else saveData.Armor2ID = "";

        if (CurrentWeapon != null) saveData.CurrentWeaponID = CurrentWeapon.Id;
        else saveData.CurrentWeaponID = "";

        return saveData;
    }
    public void LoadSaveData(CurrentUsingItemSaveData saveData)
    {
        if (_inventoryUI == null) _inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (_potionHandler == null) _potionHandler = FindFirstObjectByType<PotionHandler>();

        if (!string.IsNullOrEmpty(saveData.Potion1ID)) Potion1 = DataManager.Instance.GetConsumableItem(saveData.Potion1ID);
        else Potion1 = null;

        if (!string.IsNullOrEmpty(saveData.Potion2ID)) Potion2 = DataManager.Instance.GetConsumableItem(saveData.Potion2ID);
        else Potion2 = null;

        if (!string.IsNullOrEmpty(saveData.Weapon1ID)) Weapon1 = DataManager.Instance.GetWeapon(saveData.Weapon1ID);
        else Weapon1 = null;

        if (!string.IsNullOrEmpty(saveData.Weapon2ID)) Weapon2 = DataManager.Instance.GetWeapon(saveData.Weapon2ID);
        else Weapon2 = null;

        if (!string.IsNullOrEmpty(saveData.Armor1ID)) Armor1 = DataManager.Instance.GetWeapon(saveData.Armor1ID);
        else Armor1 = null;

        if (!string.IsNullOrEmpty(saveData.Armor2ID)) Armor2 = DataManager.Instance.GetWeapon(saveData.Armor2ID);
        else Armor2 = null;

        if (!string.IsNullOrEmpty(saveData.CurrentWeaponID)) CurrentWeapon = DataManager.Instance.GetWeapon(saveData.CurrentWeaponID);
        else CurrentWeapon = null;

        PotionAmount1 = saveData.PotionAmount1;
        PotionAmount2 = saveData.PotionAmount2;

        EquipmentData loadedCurrentWeapon = CurrentWeapon;

        _inventoryUI.InitUsingItems(
            Potion1,
            Potion2,
            Weapon1,
            Weapon2,
            Armor1,
            Armor2,
            PotionAmount1,
            PotionAmount2
        );

        _inventoryUI.UpdateCurrentUsingItems();

        CurrentWeapon = loadedCurrentWeapon;

        CurrentUsingItems currentUsingItems = FindFirstObjectByType<CurrentUsingItems>();
        if (currentUsingItems != null)
        {
            currentUsingItems.CurrentWeapon = loadedCurrentWeapon;
        }

        _potionHandler.RefreshPotion();
    }
    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Init") return;
        if (scene.name == "StartScene_LCS")
        {
            HasInitializedUsingItems = false;
            Potion1 = null; Potion2 = null;
            Weapon1 = null; Weapon2 = null;
            Armor1 = null; Armor2 = null;
            PotionAmount1 = 0; PotionAmount2 = 0;
            CurrentWeapon = null;

            for (int i = 0; i < Items.Length; i++)
            {
                Items[i] = null;
            }
            Capacity = _initialCapacity;

            return;
        }

        if (_inventoryUI == null) _inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (_potionHandler == null) _potionHandler = FindFirstObjectByType<PotionHandler>();

        if (!HasInitializedUsingItems) return;
        _inventoryUI.InitUsingItems(Potion1, Potion2, Weapon1, Weapon2, Armor1, Armor2, PotionAmount1, PotionAmount2);
        _inventoryUI.UpdateCurrentUsingItems();
        _potionHandler.RefreshPotion();
    }

    public void ResetToDefaultData()
    {
        _inventoryUI.InitUsingItems(null, null, null, null, null, null, 0, 0);
        _inventoryUI.UpdateCurrentUsingItems();
        _potionHandler.RefreshPotion();



        //// 인벤토리 칸 초기화
        //Items = new InventoryItem[_maxCapacity];
        //Capacity = _initialCapacity;

        //// 기본 장착/사용 아이템 다시 세팅
        //Potion1 = DataManager.Instance.GetConsumableItem("POT_BAG_001");
        //Potion2 = null;

        //Weapon1 = DataManager.Instance.GetWeapon("WPN_LON_001");
        //Weapon2 = DataManager.Instance.GetWeapon("WPN_AST_001");

        //Armor1 = null;
        //Armor2 = null;

        //PotionAmount1 = 3;
        //PotionAmount2 = 0;

        //CurrentWeapon = Weapon1;

        //HasInitializedUsingItems = true;

        //// UI가 존재하는 씬이면 UI도 갱신
        //if (_inventoryUI == null)
        //    _inventoryUI = FindFirstObjectByType<InventoryUI>();

        //if (_potionHandler == null)
        //    _potionHandler = FindFirstObjectByType<PotionHandler>();

        //if (_inventoryUI != null)
        //{
        //    _inventoryUI.InitUsingItems(
        //        Potion1,
        //        Potion2,
        //        Weapon1,
        //        Weapon2,
        //        Armor1,
        //        Armor2,
        //        PotionAmount1,
        //        PotionAmount2
        //    );

        //    _inventoryUI.UpdateCurrentUsingItems();
        //}

        //if (_potionHandler != null)
        //    _potionHandler.RefreshPotion();
    }
    public bool HasAnyItem()
    {
        foreach (InventoryItem item in Items)
        {
            if (item != null)
                return true;
        }

        return false;
    }

}