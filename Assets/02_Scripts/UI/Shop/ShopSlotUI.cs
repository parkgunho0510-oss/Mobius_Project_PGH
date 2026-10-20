using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class ShopSlotUI : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _name;
    [SerializeField] TMP_Text _price;
    [SerializeField] TMP_Text _stock;
    [SerializeField] Button _btnBuy;
    [SerializeField] ShopService _shopService;

    [Header("Data")]
    [SerializeField] ShopEntryData _data;

    [Header("Lock")]
    [SerializeField] private GameObject _lockPanel;

    private bool _isLocked;

    private ShopNpc _shopNpc;


    int _amount;

    void Awake()
    {
        if (_icon == null)          Debug.Log("Icon is NULL");
        if (_name == null)          Debug.Log("Name Text is NULL");
        if (_price == null)         Debug.Log("Price Text is NULL");
        if (_stock == null)         Debug.Log("Stock Text is NULL");
        if (_btnBuy == null)        Debug.Log("Buy Button is NULL");
        if (_shopService == null)   _shopService = FindFirstObjectByType<ShopService>();
        if (_btnBuy != null) _btnBuy.onClick.AddListener(() => OnClickBuyBtn());
    }
    void OnEnable()
    {
        //if(!_stock.gameObject.activeSelf) _stock.gameObject.SetActive(true);
        //if(_data != null) Init(_data);
    }
    void OnDestroy()
    {
        if (_btnBuy != null) _btnBuy.onClick.RemoveAllListeners();
    }
    void OnClickBuyBtn()
    {
        if(_shopService.TryBuy(_data, 1))
        {
            _amount -= 1;
            RefreshUI();
        }
    }
    void RefreshUI()
    {
        if (_data.InfiniteStock) return;

        if (_amount <= 0) gameObject.SetActive(false);
        else _stock.SetText($"Amount : {_amount}");
    }
    public void InitSlot(ShopEntryData data, ShopNpc shopNpc)
    {
        _data = data;
        _shopNpc = shopNpc;

        _isLocked = IsLocked();

        if (_lockPanel != null)
            _lockPanel.SetActive(_isLocked);

        if (data.OverrideBasePrice)
            _price.SetText($"{data.Item.BasePrice}G");
        else
            _price.SetText($"Price : {data.Price}G");

        _name.SetText(data.Item.ItemName);
        _icon.sprite = data.Item.Icon;
        _amount = data.Stock;

        if (_data.InfiniteStock)
        {
            _stock.gameObject.SetActive(false);
        }
        else
        {
            _stock.gameObject.SetActive(true);
            _stock.SetText($"Amount : {_amount}");
        }
    }
    private bool IsLocked()
    {
        if (_data == null)
            return true;

        
        if (_shopNpc == null)
            return false;

        return _shopNpc.RunTimeState.NpcStateType < _data.NpcStateType;
    }


    public void RefreshLockState()
    {
        _isLocked = IsLocked();

        if (_lockPanel != null)
            _lockPanel.SetActive(_isLocked);
    }

}
