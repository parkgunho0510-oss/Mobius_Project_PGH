using UnityEngine;
using UnityEngine.SceneManagement;
public class ShopService : MonoBehaviour
{
    
    [SerializeField] Inventory _inventory;
    [SerializeField] Wallet _wallet;
    void Awake()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }
    void Start()
    {
        if (_inventory == null) _inventory = FindFirstObjectByType<Inventory>();
    }

    public bool TryBuy(ShopEntryData data, int amount = 1)
    {
        int totalPrice = (data.OverrideBasePrice) ? data.Item.BasePrice * amount : data.Price * amount;

        if (!_wallet.HasEnough(totalPrice)) return false;

        int addedAmount = _inventory.Add(data.Item, amount);
        if (addedAmount > 0)
        {
            QuestEvents.RaiseCollectItem(data.Item, amount - addedAmount);
            return false;
        }
        QuestEvents.RaiseCollectItem(data.Item, amount);
        _wallet.SpendGold(totalPrice);

        return true;
    }

    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Init") return;
        if (scene.name == "StartScene_LCS") return;

        if (_wallet == null) _wallet = FindFirstObjectByType<Wallet>();
    }
}
