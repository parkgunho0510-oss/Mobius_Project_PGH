using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class StartMenuUI : MonoBehaviour
{
    [Header("Main Buttons")]
    [SerializeField] private Button _btnStart;
    [SerializeField] private Button _btnLoad;
    [SerializeField] private Button _btnExit;
    [SerializeField] private Button _btnHelp;
    [SerializeField] private Button _btnSetting;

    [Header("Popup UI")]
    [SerializeField] private GameObject _settingPopup;
    [SerializeField] private GameObject _helpPopup;

    [Header("Confirm Popup")]
    [SerializeField] private LoadConfirmUI _loadConfirmPopup;
    [SerializeField] private ExitConfirmUI _exitConfirmPopup;
    [SerializeField] private CanNotLoadConfirmPopupUI _canNotLoadConfirmPopup;

    [Header("Help Popup Buttons")]
    [SerializeField] private Button _btnHelpClose;

    [Header("Setting Popup Buttons")]
    [SerializeField] private Button _btnSettingClose;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }
    void Start()
    {
        if (_btnStart == null)          Debug.Log("Start Btn is Null");
        if (_btnLoad == null)           Debug.Log("Load Btn is Null");
        if (_btnExit == null)           Debug.Log("Exit Btn is NULL");
        if (_btnHelp == null)           Debug.Log("Help Btn  is NULL");
        if (_btnSetting == null)        Debug.Log("Setting Btn is Null");
        if (_btnHelpClose == null)      Debug.Log("Help Close Btn is NULL");
        if (_btnSettingClose == null)   Debug.Log("Setting Close Btn is NULL");

        if(_loadConfirmPopup == null)   _loadConfirmPopup = FindFirstObjectByType<LoadConfirmUI>();
        if(_exitConfirmPopup == null)   _exitConfirmPopup = FindFirstObjectByType<ExitConfirmUI>();
        if (_canNotLoadConfirmPopup== null) _canNotLoadConfirmPopup = FindFirstObjectByType<CanNotLoadConfirmPopupUI>();

        _btnStart.onClick.AddListener(OnClickStart);
        _btnLoad.onClick.AddListener(OnClickLoad);
        _btnExit.onClick.AddListener(OnClickExit);
        _btnHelp.onClick.AddListener(OnClickHelp);
        _btnSetting.onClick.AddListener(OnClickSetting);

        _btnHelpClose.onClick.AddListener(CloseAllPopup);
        _btnSettingClose.onClick.AddListener(CloseAllPopup);
    }
    private void OnDestroy()
    {
        
        _btnStart.onClick.RemoveListener(OnClickStart);
        _btnLoad.onClick.RemoveListener(OnClickLoad);
        _btnExit.onClick.RemoveListener(OnClickExit);
        _btnHelp.onClick.RemoveListener(OnClickHelp);
        _btnSetting.onClick.RemoveListener(OnClickSetting);

        _btnHelpClose.onClick.AddListener(CloseAllPopup);
        _btnSettingClose.onClick.AddListener(CloseAllPopup);
    }

    private void OnClickStart()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.ResetForNewGame();

        if (QuestManager.Instance != null)
            QuestManager.Instance.ResetForNewGame();

        //InventoryManager.Instance.ResetToDefaultData();
        StatManager.Instance.InitStats();
        GoldManager.Instance.InitGold();

        SceneManager.LoadScene("Tutorial");

    }

    private void OnClickLoad()
    { 
        CloseAllPopup();

        if (!SaveManager.Instance.CanLoadGame())
        {
            _canNotLoadConfirmPopup.Show();           
            return;
        }

          _loadConfirmPopup.Show(); 
   
    }

    private void OnClickExit()
    {
        CloseAllPopup();
        _exitConfirmPopup.Show();
    }

    private void OnClickHelp()
    {
        CloseAllPopup();
        _helpPopup.SetActive(true);
    }

    private void OnClickSetting()
    {
        CloseAllPopup();
        _settingPopup.SetActive(true);
    }



    private void CloseAllPopup()
    {
        _loadConfirmPopup.Hide();
        _exitConfirmPopup.Hide();
        _settingPopup.SetActive(false);
        _helpPopup.SetActive(false);
    }

    public void OnEscape()
    {
        CloseAllPopup();
    }

    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_loadConfirmPopup == null) _loadConfirmPopup = FindFirstObjectByType<LoadConfirmUI>();
        if (_exitConfirmPopup == null) _exitConfirmPopup = FindFirstObjectByType<ExitConfirmUI>();
    }
}