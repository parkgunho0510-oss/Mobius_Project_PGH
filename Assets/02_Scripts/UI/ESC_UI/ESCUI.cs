using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ESCUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _btnSetting;
    [SerializeField] private Button _btnHelp;
    [SerializeField] private Button _btnSave;
    [SerializeField] private Button _btnResume;
    [SerializeField] private Button _btnExitToStart;

    [Header("Popup UI")]
    [SerializeField] private GameObject _settingPopup;
    [SerializeField] private GameObject _helpPopup;
    [SerializeField] private GameObject _saveconfirmPopup;

    [SerializeField] private GameObject _cantSaveConfirmPopup;
    [SerializeField] private GameObject _exitConfirmPopup;


    [Header("Controller")]
    [SerializeField] private ESCUIController _escUIController;
    [SerializeField] private ExitConfirmPopupUI _exitConfirmPopupUI;

    private const string LobbySceneName = "Lobby";

    private void Awake()
    {
        if (_escUIController == null)
        {
            _escUIController = FindFirstObjectByType<ESCUIController>();
        }

        if (_exitConfirmPopupUI == null)
        {
            _exitConfirmPopupUI = FindFirstObjectByType<ExitConfirmPopupUI>();
        }

        _btnSetting.onClick.AddListener(OnClickSetting);
        _btnHelp.onClick.AddListener(OnClickHelp);
        _btnSave.onClick.AddListener(OnClickSave);
        _btnResume.onClick.AddListener(OnClickResume);
        _btnExitToStart.onClick.AddListener(OnClickExitPopup);
    }

    private void OnDestroy()
    {
        _btnSetting.onClick.RemoveListener(OnClickSetting);
        _btnHelp.onClick.RemoveListener(OnClickHelp);
        _btnSave.onClick.RemoveListener(OnClickSave);
        _btnResume.onClick.RemoveListener(OnClickResume);
        _btnExitToStart.onClick.RemoveListener(OnClickExitPopup);
    }

    private void OnClickSetting()
    {
        CloseSubPopups();
        _settingPopup.SetActive(true);
    }

    private void OnClickHelp()
    {
        CloseSubPopups();
        _helpPopup.SetActive(true);
    }

    private void OnClickSave()
    {
        CloseSubPopups();

        string currentSceneName = SceneManager.GetActiveScene().name;

        // �κ���� �ƴ� ��� ���� �Ұ� �˾�
        if (currentSceneName != LobbySceneName)
        {
            if (_cantSaveConfirmPopup != null)
                _cantSaveConfirmPopup.SetActive(true);

            Debug.Log("���� �������������� ������ �� �����ϴ�.");
            return;
        }

        // �κ���� ��� ���� Ȯ�� �˾�
        if (_saveconfirmPopup != null)
            _saveconfirmPopup.SetActive(true);
    }
    public void OnClickConfirmSave()
    {
        SaveManager.Instance.SaveGame();

        Debug.Log("Save Game");

        if (_saveconfirmPopup != null)
            _saveconfirmPopup.SetActive(false);
    }

    public void OnClickCancelSave()
    {
        if (_saveconfirmPopup != null)
            _saveconfirmPopup.SetActive(false);
    }

    public void OnClickCloseCantSavePopup()
    {
        if (_cantSaveConfirmPopup != null)
            _cantSaveConfirmPopup.SetActive(false);
    }

private void OnClickResume()
    {
        _escUIController.CloseESCPanel();
    }

    public void OnClickExitToStart()
    {
        Time.timeScale = 1f;

        if (SaveManager.Instance != null)
            SaveManager.Instance.ResetForNewGame();

        if (QuestManager.Instance != null)
            QuestManager.Instance.ResetForNewGame();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ResetToDefaultData();

        if (StatManager.Instance != null)
            StatManager.Instance.InitStats();

        if (GoldManager.Instance != null)
            GoldManager.Instance.InitGold();

        SceneManager.LoadScene("StartScene_LCS");
    }

    private void CloseSubPopups()
    {
        if (_settingPopup != null)
            _settingPopup.SetActive(false);

        if (_helpPopup != null)
            _helpPopup.SetActive(false);

        if (_saveconfirmPopup != null)
            _saveconfirmPopup.SetActive(false);

        if (_cantSaveConfirmPopup != null)
            _cantSaveConfirmPopup.SetActive(false);

        if (_exitConfirmPopupUI != null)
            _exitConfirmPopup.SetActive(false);
    }

    private void OnClickExitPopup()
    {
        CloseSubPopups();

        if (_exitConfirmPopupUI != null)
            _exitConfirmPopupUI.Open();
    }

}