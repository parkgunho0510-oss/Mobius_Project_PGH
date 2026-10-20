using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
public class CurrentUsingItems : MonoBehaviour
{

    [SerializeField] private InventoryUI _inventoryUI;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private PlayerStatus _playerStatus;
    [SerializeField] private StatusUI _statusUI;

    public event Action OnItemsInitialized;
    public bool IsDataLoaded { get; private set; } = false;

    private ItemData _defaultPotionItem1;
    private ItemData _defaultPotionItem2;
    private ItemData _defaultWeaponItem1;
    private ItemData _defaultWeaponItem2;
    private ItemData _defaultArmorItem1;
    private ItemData _defaultArmorItem2;



    private EquipmentData _weapon1;
    private EquipmentData _weapon2;
    private EquipmentData _armor1;
    private EquipmentData _armor2;
    private ConsumableData _potion1;
    private ConsumableData _potion2;
    private int _potionAmount1;
    private int _potionAmount2;


    private EquipmentData _currentWeapon;

    public EquipmentData CurrentWeapon
    {
        get => _currentWeapon;
        set => _currentWeapon = value;
    }


    public EquipmentData Weapon1
    {
        get => _weapon1;
        set => _weapon1 = value;
    }

    public EquipmentData Weapon2
    {
        get => _weapon2;
        set => _weapon2 = value;
    }

    public EquipmentData Armor1
    {
        get => _armor1;
        set => _armor1 = value;
    }

    public EquipmentData Armor2
    {
        get => _armor2;
        set => _armor2 = value;
    }


    public ConsumableData Potion1
    {
        get => _potion1;
        set => _potion1 = value;
    }

    public int PotionAmount1
    {
        get => _potionAmount1;
        set => _potionAmount1 = value;
    }

    public ConsumableData Potion2
    {
        get => _potion2;
        set => _potion2 = value;
    }

    public int PotionAmount2
    {
        get => _potionAmount2;
        set => _potionAmount2 = value;
    }




    private void Awake()
    {
        if (_inventory == null)
        {
            _inventory = GetComponent<Inventory>();
        }
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }
    private IEnumerator Start()
    {

        yield return null;

        


    }


    public void SetUsingItems(
        EquipmentData weapon1,
        EquipmentData weapon2,
        EquipmentData armor1,
        EquipmentData armor2,
        ConsumableData potion1,
        int potionAmount1,
        ConsumableData potion2,
        int potionAmount2
        
         )
    {
       

        Debug.Log($"SetUsingItems 포션: {potion1}, 수량: {potionAmount1}");
        Debug.Log($"SetUsingItems 포션: {potion2}, 수량: {potionAmount2}");

        _weapon1 = weapon1;
        _weapon2 = weapon2;
        _armor1 = armor1;
        _armor2 = armor2;
        _potion1 = potion1;
        _potion2 = potion2;
        _potionAmount1 = potionAmount1;
        _potionAmount2 = potionAmount2;

        if (_currentWeapon == null || (_currentWeapon != _weapon1 && _currentWeapon != _weapon2))
        {
            _currentWeapon = _weapon1;
        }


        SaveUsingItemsToManager();

    }
    public EquipmentData GetMainWeaponData()
    {
        return _currentWeapon;
    }

    public EquipmentData GetSubWeaponData()
    {
        return _weapon2;
    }


    //public void DecreasePotionAmount()
    //{
    //    _potionAmount1--;

    //    if (_potionAmount1 <= 0)
    //    {
    //        _potionAmount1 = 0;
    //        _potion1 = null;
    //    }
    //}
    public EquipmentData GetSubWeapon()
    {
        if (CurrentWeapon == Weapon1)
            return Weapon2;

        return Weapon1;
    }


    public void InitInvenItem()
    {
        InventoryManager.Instance.HasInitializedUsingItems = true;

        UIEvents.RaiseWeaponEquipped(CurrentWeapon, GetSubWeaponData());
    }


    public void SaveUsingItemsToManager()
    {


        InventoryManager.Instance.Potion1 = Potion1;
        InventoryManager.Instance.Potion2 = Potion2;
        InventoryManager.Instance.Weapon1 = Weapon1;
        InventoryManager.Instance.Weapon2 = Weapon2;
        InventoryManager.Instance.Armor1 = Armor1;
        InventoryManager.Instance.Armor2 = Armor2;
        InventoryManager.Instance.PotionAmount1 = PotionAmount1;
        InventoryManager.Instance.PotionAmount2 = PotionAmount2;
        InventoryManager.Instance.CurrentWeapon = CurrentWeapon;
    }

    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Init") return;
        if (scene.name == "StartScene_LCS") return;

        if (_inventoryUI == null)
        {
            _inventoryUI = FindFirstObjectByType<InventoryUI>();
        }
        if (_playerStatus == null)
        {
            _playerStatus = FindFirstObjectByType<PlayerStatus>();
        }
        if (_statusUI == null)
        {
            _statusUI = FindFirstObjectByType<StatusUI>();
        }


        if (InventoryManager.Instance.HasInitializedUsingItems == false)
        {

            _defaultWeaponItem1 = DataManager.Instance.GetWeapon("WPN_LON_001");
            _defaultWeaponItem2 = DataManager.Instance.GetWeapon("WPN_LON_001");
            //_defaultWeaponItem1 = DataManager.Instance.GetWeapon("WPN_AST_001");
            //_defaultWeaponItem2 = DataManager.Instance.GetWeapon("WPN_IGN_001");

            _defaultPotionItem1 = DataManager.Instance.GetConsumableItem("POT_BAG_001");
            _defaultPotionItem2 = null;
            _defaultArmorItem1 = DataManager.Instance.GetWeapon("ACC_SLI_001");
            _defaultArmorItem2 = null;
            PotionAmount1 = 0;
            PotionAmount2 = 0;
            CurrentWeapon = null;



            //_inventoryUI.InitUsingItems(
            //    _defaultPotionItem,
            //    _defaultWeaponItem1,
            //    _defaultWeaponItem2,
            //    _defaultArmorItem
            //    , 3);

            Weapon1 = _defaultWeaponItem1 as EquipmentData;
            Weapon2 = _defaultWeaponItem2 as EquipmentData;
            Armor1 = _defaultArmorItem1 as EquipmentData;
            Armor2 = _defaultArmorItem2 as EquipmentData;
            CurrentWeapon = _defaultWeaponItem1 as EquipmentData;
            Potion1 = _defaultPotionItem1 as ConsumableData;
            PotionAmount1 = 3;

            //_playerStatus.ApplyEquipmentStat(this);
            //_inventoryUI.UpdateCurrentUsingItems();


            InitInvenItem();
            SaveUsingItemsToManager();

            InventoryManager.Instance.HasInitializedUsingItems = true;

        }
        else
        {
            Weapon1 = InventoryManager.Instance.Weapon1;
            Weapon2 = InventoryManager.Instance.Weapon2;
            Armor1 = InventoryManager.Instance.Armor1;
            Armor2 = InventoryManager.Instance.Armor2;
            Potion1 = InventoryManager.Instance.Potion1;
            PotionAmount1 = InventoryManager.Instance.PotionAmount1;
            CurrentWeapon = InventoryManager.Instance.CurrentWeapon;

            if (CurrentWeapon == null)
                CurrentWeapon = Weapon1;

            //_inventoryUI.InitUsingItems(
            //    Potion,
            //    Weapon1,
            //    Weapon2,
            //    Armor,
            //    PotionAmount);

            _inventoryUI.UpdateCurrentUsingItems();

           
        }

        _playerStatus.ApplyEquipmentStat(this);
        _statusUI.RefreshAllUI();

        UIEvents.RaiseWeaponEquipped(CurrentWeapon, GetSubWeaponData());

        IsDataLoaded = true;
        OnItemsInitialized?.Invoke();
        Debug.Log("Scene Loaded");
    }
}
