using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WalletService : MonoBehaviour
{
    [SerializeField] private TMP_Text _currentIvenGoldText;
    [SerializeField] private TMP_Text _currentMainGoldText;
    [SerializeField] private Wallet _wallet;

    void Awake()
    {
        UIEvents.OnGoldChanged += UpdateGoldText;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }
    private void OnDestroy()
    {
        UIEvents.OnGoldChanged -= UpdateGoldText;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }
    private void UpdateGoldText(int gold)
    {
        if (_currentIvenGoldText == null || _currentMainGoldText == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();

            TMP_Text[] texts = canvas.GetComponentsInChildren<TMP_Text>(true);

            foreach (TMP_Text text in texts)
            {
                if (text.gameObject.CompareTag("InventoryGoldText"))
                {
                    _currentIvenGoldText = text;
                }
                if (text.gameObject.CompareTag("MainGoldText"))
                {
                    _currentMainGoldText = text;
                }
            }
        }
        _currentIvenGoldText.text = $"º¸À¯°ñµå : {gold}G";
        _currentMainGoldText.text = $"GOLD : {gold}G";
    }

    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Init") return;
        if (scene.name == "StartScene_LCS") return;

        _wallet = FindFirstObjectByType<Wallet>();

        if (_wallet != null)
            UpdateGoldText(GoldManager.Instance.Gold);
    }
}
