using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;
public class ShopUI : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] GameObject _root;

    [Header("Slot Parent - Viewport_Content")]
    [SerializeField] GameObject _equip_slotParent;
    [SerializeField] GameObject _consum_slotParent;
    [SerializeField] private GameObject _resell_slotParent;

    [SerializeField] Button _btnClose;
    [SerializeField] Button _btnEquipTab;
    [SerializeField] Button _btnConsumTab;
    [SerializeField] Button _btnResellTab;


    [SerializeField] GameObject _shopSlotUIPrefab;
    [SerializeField] string _linkedID = "NPC_SHOP";

    [Header("DATA")]
     ShopData _data;
    [SerializeField] private string _shopDataId = "SHP_STAGE_001";

    [Header("NPC State")]
    [SerializeField] private ShopNpc _shopNpc;

    private bool _isGenerated;
    private readonly List<ShopSlotUI> _shopSlots = new();


    void Awake()
    {
        if (_shopNpc == null)
            _shopNpc = FindFirstObjectByType<ShopNpc>();

        if (_root == null)
            _root = transform.GetChild(0).gameObject;

        if (_equip_slotParent == null)
            Debug.Log("Equip Slot Parent is NULL");

        if (_consum_slotParent == null)
            Debug.Log("Consumable Slot Parent is NULL");

        if (_btnClose == null)
            Debug.Log("Close Button is NULL");

        if (_shopSlotUIPrefab == null)
            Debug.Log("Shop Slot Prefab is NULL");

        if (_data == null)
            Debug.Log("ShopData is NULL");

        _data = DataManager.Instance.GetShop(_shopDataId);
        _data.LinkData(DataManager.Instance);

        Debug.Log($"ShopData ID: {_data.ID}");
        Debug.Log($"ShopId: {_data.ShopId}");
        Debug.Log($"Entries Count: {_data.Entries.Count}");


        Init();
        //GenerateShopSlots();

        UIEvents.OnDialogueEndedEvent += HandleDialogueEnded;
    }

    void Start()
    {
        Hide();
        ShowConsumableShop();
    }

    void OnDestroy()
    {
        if (_btnClose != null)
            _btnClose.onClick.RemoveListener(Hide);

        if (_btnEquipTab != null)
            _btnEquipTab.onClick.RemoveListener(OnClickEquipmentTab);

        if (_btnConsumTab != null)
            _btnConsumTab.onClick.RemoveListener(OnClickConsumableTab);

        if (_btnResellTab != null)
            _btnResellTab.onClick.RemoveListener(OnClickResellTab);

        UIEvents.OnDialogueEndedEvent -= HandleDialogueEnded;
    }

    void Init()
    {
        if (_btnClose != null)
            _btnClose.onClick.AddListener(Hide);

        if (_btnEquipTab != null)
            _btnEquipTab.onClick.AddListener(OnClickEquipmentTab);

        if (_btnConsumTab != null)
            _btnConsumTab.onClick.AddListener(OnClickConsumableTab);

        if (_btnResellTab != null)
            _btnResellTab.onClick.AddListener(OnClickResellTab);
    }

    void GenerateShopSlots()
    {
        if (_data == null)
            return;

        for (int i = 0; i < _data.Entries.Count; i++)
        {
            ShopEntryData entry = _data.Entries[i];

            Transform parent = null;

            if (entry.Item.Type == ItemType.EQUIPMENT)
                parent = _equip_slotParent.transform;
            else if (entry.Item.Type == ItemType.CONSUMABLE)
                parent = _consum_slotParent.transform;
            else
                continue;

            GameObject shopSlotPref = Instantiate(_shopSlotUIPrefab, parent);

            ShopSlotUI slot = shopSlotPref.GetComponent<ShopSlotUI>();
            slot.InitSlot(entry, _shopNpc);

            _shopSlots.Add(slot);
        }
    }

    public void AddResellSlot(ShopEntryData data)
    {
        GameObject obj = Instantiate(_shopSlotUIPrefab, _resell_slotParent.transform);

        ShopSlotUI slot = obj.GetComponent<ShopSlotUI>();
        slot.InitSlot(data, null);
    }

    void ClearShopSlots() // when add more shopNPC
    {
        for (int i = _equip_slotParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_equip_slotParent.transform.GetChild(i).gameObject);
        }

        for (int i = _consum_slotParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_consum_slotParent.transform.GetChild(i).gameObject);
        }
    }





    public void Hide()
    {
        _root.SetActive(false);

        UIEvents.RaiseInventoryClosed();
        UIEvents.RaiseCursorHide();
        UIEvents.RaiseUIClosed();
    }

    void Show()
    {
        _root.SetActive(true);

        UIEvents.RaiseInventoryOpened();
        UIEvents.RaiseCursorShowed();

        ShowConsumableShop();

        if (!_isGenerated)
        {
            GenerateShopSlots();
            _isGenerated = true;
        }
        else
        {
            RefreshShopSlots();
        }
    }
    private void RefreshShopSlots()
    {
        for (int i = 0; i < _shopSlots.Count; i++)
        {
            _shopSlots[i].RefreshLockState();
        }
    }



    void HandleDialogueEnded(string id)
    {
        if (_linkedID == id)
        {
            Show();
        }
    }

    public void OnClickEquipmentTab()
    {
        ShowEquipmentShop();
    }

    public void OnClickConsumableTab()
    {
        ShowConsumableShop();
    }
    public void OnClickResellTab()
    {
        ShowResellShop();
    }

    void ShowEquipmentShop()
    {
        _equip_slotParent.SetActive(true);
        _consum_slotParent.SetActive(false);
        _resell_slotParent.SetActive(false);
    }

    void ShowConsumableShop()
    {
        _equip_slotParent.SetActive(false);
        _consum_slotParent.SetActive(true);
        _resell_slotParent.SetActive(false);
    }

    void ShowResellShop()
    {
        _equip_slotParent.SetActive(false);
        _consum_slotParent.SetActive(false);
        _resell_slotParent.SetActive(true);
    }
}