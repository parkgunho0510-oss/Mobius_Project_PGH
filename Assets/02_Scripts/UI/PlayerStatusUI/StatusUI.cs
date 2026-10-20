using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class StatusUI : MonoBehaviour
{

    [SerializeField] PlayerStatus _playerStatus;
    [SerializeField] private ResetStatConfirmPopup _resetPopup;
    [SerializeField] GameObject _playerStatusUI_root;


    [Header("Stat Amount Text")]
    [SerializeField] private TMP_Text _hpStatsText;
    [SerializeField] private TMP_Text _staminaStatsText;
    [SerializeField] private TMP_Text _ATKStatsText;
    [SerializeField] private TMP_Text _DEFStatsText;
    [SerializeField] private TMP_Text _LUCKStatsText;

    [SerializeField] private TMP_Text _statsPointText;
    private CurrentUsingItems _currentUsingItems;


    [Header("Stat Level Text")]
    [SerializeField] private TMP_Text _hpLevel;
    [SerializeField] private TMP_Text _staminaLevel;
    [SerializeField] private TMP_Text _ATKLevel;
    [SerializeField] private TMP_Text _DEFLevel;
    [SerializeField] private TMP_Text _LUCKLevel;


    [Header("Stat Reset Count Text")]
    [SerializeField] private TMP_Text _statResetCountText;


    [Header("Buttons")]
    [SerializeField] List<Button> _buttons = new();

    private void Awake()
    {
        if (_playerStatusUI_root == null) _playerStatusUI_root = transform.GetChild(0).gameObject;

        
        _playerStatusUI_root.SetActive(false);

        UIEvents.OnStatusOpenedEvent += HandleStatusOpened;
        UIEvents.OnStatusClosedEvent += HandleStatusClosed;

        _buttons[0].onClick.AddListener(() => OnClickIncreaseHP());
        _buttons[1].onClick.AddListener(() => OnClickIncreaseStamina());
        _buttons[2].onClick.AddListener(() => OnClickIncreaseATK());
        _buttons[3].onClick.AddListener(() => OnClickIncreaseDEF());
        _buttons[4].onClick.AddListener(() => OnClickReset());

        if (_playerStatus == null)
        {
            _playerStatus = FindFirstObjectByType<PlayerStatus>();
        }
        if (_resetPopup == null)
        {
            _resetPopup = GetComponentInChildren<ResetStatConfirmPopup>(true);
        }
        _currentUsingItems = FindFirstObjectByType<CurrentUsingItems>();
    }
    private IEnumerator Start()
    {
        yield return null;

        if (_resetPopup == null)
        {
            _resetPopup = GetComponentInChildren<ResetStatConfirmPopup>(true);
        }

        if (_currentUsingItems != null)
        {
            _currentUsingItems.OnItemsInitialized += RefreshAllUI;
        }

     if (_currentUsingItems != null && _currentUsingItems.IsDataLoaded && _playerStatus != null)
            {
                _playerStatus.ApplyEquipmentStat(_currentUsingItems);
                RefreshAllUI();
        }
    }
    private void OnEnable()
    {
        UIEvents.OnStatPointChanged += UpdateStatPoint;

        UIEvents.OnMaxHPChanged += UpdateHPStats;
        UIEvents.OnMaxStaminaChanged += UpdateStaminaStats;
        UIEvents.OnATKStatChanged += UpdateATKStats;
        UIEvents.OnDEFStatChanged += UpdateDEFStats;
        UIEvents.OnLUCKStatChanged += UpdateLUCKStats;

        UIEvents.OnStatLevelChanged += UpdateStatLevelText;
        UIEvents.OnStatResetCountChanged += UpdateStatResetCountText;

        if (_currentUsingItems == null)
            _currentUsingItems = FindFirstObjectByType<CurrentUsingItems>();

        if (_playerStatus == null)
            _playerStatus = FindFirstObjectByType<PlayerStatus>();

        if (_currentUsingItems != null)
        {
            _currentUsingItems.OnItemsInitialized += RefreshAllUI;

            if (_currentUsingItems.IsDataLoaded && _playerStatus != null)
            {
                _playerStatus.ApplyEquipmentStat(_currentUsingItems);
                RefreshAllUI();
            }
        }
    }

    private void OnDisable()
    {
        UIEvents.OnStatPointChanged -= UpdateStatPoint;

        UIEvents.OnMaxHPChanged -= UpdateHPStats;
        UIEvents.OnMaxStaminaChanged -= UpdateStaminaStats;
        UIEvents.OnATKStatChanged -= UpdateATKStats;
        UIEvents.OnDEFStatChanged -= UpdateDEFStats;
        UIEvents.OnLUCKStatChanged -= UpdateLUCKStats;

        UIEvents.OnStatLevelChanged -= UpdateStatLevelText;
        UIEvents.OnStatResetCountChanged -= UpdateStatResetCountText;

        if (_currentUsingItems != null)
        {
            _currentUsingItems.OnItemsInitialized -= RefreshAllUI;
        }
    }
    void OnDestroy()
    {
        foreach (var item in _buttons)
        {
            item.onClick.RemoveAllListeners();
        }
        UIEvents.OnStatusOpenedEvent -= HandleStatusOpened;
        UIEvents.OnStatusClosedEvent -= HandleStatusClosed;

        if (_currentUsingItems != null)
            _currentUsingItems.OnItemsInitialized -= RefreshAllUI;
    }
    private void EnsureReferences()
    {
        if (_playerStatus == null)
            _playerStatus = FindFirstObjectByType<PlayerStatus>();
    }

    void Show()
    {
        _playerStatusUI_root.SetActive(true);
        RefreshLevelUpButtonState();
    }
    void Hide()
    {

        _playerStatusUI_root.SetActive(false);
    }
    private void UpdateStatPoint(int point)
    {
        if (_statsPointText == null)
        {
            Debug.LogError("StatusUI: _statsPointText가 연결 안 됨");
            return;
        }

        _statsPointText.text = $"남은 스탯포인트: {point}";
    }



    private void UpdateHPStats(float maxHP)
    {
        _hpStatsText.text = $": {maxHP}";
    }

    private void UpdateStaminaStats(float maxStamina)
    {
        _staminaStatsText.text = $": {maxStamina}";
    }

    private void UpdateATKStats(float ATK)
    {
        _ATKStatsText.text = $": {ATK}";
    }


    private void UpdateDEFStats(float DEF)
    {
        _DEFStatsText.text = $": {DEF}";
    }


    private void UpdateLUCKStats(float LUCK)
    {
        _LUCKStatsText.text = $": {LUCK}";
    }



    public void OnClickIncreaseHP()
    {
        _playerStatus.TryIncreaseStat(StatType.HP);
    }

    public void OnClickIncreaseStamina()
    {
        _playerStatus.TryIncreaseStat(StatType.STAMINA);
    }

    public void OnClickIncreaseATK()
    {
        _playerStatus.TryIncreaseStat(StatType.ATK);
    }

    public void OnClickIncreaseDEF()
    {
        _playerStatus.TryIncreaseStat(StatType.DEF);
    }


    public void OnClickIncreaseLUCK()
    {
        _playerStatus.TryIncreaseStat(StatType.LUCK);
    }


    public void OnClickReset()
    {
        if (_playerStatus.StatResetCount <= 0)
        {
            Debug.Log("초기화권 없음");
            return;
        }
            _resetPopup.Open();
    }

    private void UpdateStatLevelText(StatType statType, int level)
    {
        switch (statType)
        {
            case StatType.HP:
                _hpLevel.text = $"Lv.{level}";
                break;

            case StatType.STAMINA:
                _staminaLevel.text = $"Lv.{level}";
                break;

            case StatType.ATK:
                _ATKLevel.text = $"Lv.{level}";
                break;

            case StatType.DEF:
                _DEFLevel.text = $"Lv.{level}";
                break;

            case StatType.LUCK:
                _LUCKLevel.text = $"Lv.{level}";
                break;
        }
    }
    public void RefreshAllUI()
    {
        EnsureReferences();

        if (_playerStatus == null)
        {
            Debug.LogError("StatusUI: _playerStatus null");
            return;
        }

        if (_statsPointText == null)
        {
            Debug.LogError("StatusUI: _statsPointText null");
            return;
        }

        UpdateStatPoint(_playerStatus.StatPoint);

        UpdateHPStats(_playerStatus.TotalHP);
        UpdateStaminaStats(_playerStatus.TotalSTAMINA);
        UpdateATKStats(_playerStatus.TotalATK);
        UpdateDEFStats(_playerStatus.TotalDEF);
        UpdateLUCKStats(_playerStatus.TotalLUCK);

        UpdateStatLevelText(StatType.HP, _playerStatus.HPLevel);
        UpdateStatLevelText(StatType.STAMINA, _playerStatus.StaminaLevel);
        UpdateStatLevelText(StatType.ATK, _playerStatus.ATKLevel);
        UpdateStatLevelText(StatType.DEF, _playerStatus.DEFLevel);
        UpdateStatLevelText(StatType.LUCK, _playerStatus.LUCKLevel);
    }


    private void UpdateStatResetCountText(int count)
    {
        _statResetCountText.text = $"스탯 초기화 가능 횟수 :  {count}";
    }


    void HandleStatusOpened()
    {
        Show();
    }
    void HandleStatusClosed()
    {
        Hide();
    }

    private void RefreshLevelUpButtonState()
    {
        bool canLevelUpButtonClick = SceneManager.GetActiveScene().name == "Lobby";

        
        for (int i = 0; i < 4; i++)
        {
            _buttons[i].gameObject.SetActive(canLevelUpButtonClick);
        }

       
        _buttons[4].interactable = true;
    }


}
