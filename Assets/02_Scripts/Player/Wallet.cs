using UnityEngine;

public class Wallet : MonoBehaviour
{
    private int Gold
    {
        get => GoldManager.Instance.Gold;
        set => GoldManager.Instance.Gold = value;
    }

    private void Start()
    {
        UIEvents.RaiseGoldChanged(Gold);
    }

    public bool HasEnough(int price) => Gold >= price;

    public void SpendGold(int price)
    {
        Gold -= price;
        UIEvents.RaiseGoldChanged(Gold);
    }

    public void AddGold(int gold)
    {
        Gold += gold;
        UIEvents.RaiseGoldChanged(Gold);
    }

    public void AddDropGold(int dropGold)
    {
        Gold += dropGold;
        UIEvents.RaiseGoldChanged(Gold);
    }
}