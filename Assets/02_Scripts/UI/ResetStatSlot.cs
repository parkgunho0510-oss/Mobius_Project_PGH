using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResetStatSlot : MonoBehaviour
{
    [SerializeField] private Wallet _wallet;
    [SerializeField] private PlayerStatus _playerStatus;


    [Header("Components")]
    [SerializeField] private Button _buyResetButton;
    
    [SerializeField] private TMP_Text _priceText;

    [SerializeField] private int _price =5000;


    private void Awake()
    {
        if (_wallet == null)
            _wallet = FindFirstObjectByType<Wallet>();
        if (_playerStatus == null)
            _playerStatus = FindFirstObjectByType<PlayerStatus>();

        _buyResetButton.onClick.AddListener(OnClickBuy);
        _priceText.text = $"{_price} 골드";
    }
    private void OnDestroy()
    {
        _buyResetButton.onClick.RemoveListener(OnClickBuy);
    }

    private void OnClickBuy()
    {
        if (!_wallet.HasEnough(_price))
        {
            Debug.Log("골드가 부족합니다.");
            return;
        }

        _wallet.SpendGold(_price);
       _playerStatus.AddStatResetCount(1);

        Debug.Log("스탯 초기화권 구매 완료");
    }
}
