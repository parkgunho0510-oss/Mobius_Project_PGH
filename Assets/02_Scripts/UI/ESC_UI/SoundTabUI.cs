using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundTabUI : MonoBehaviour
{
    private const string MASTER_VOLUME_KEY = "MasterVolumeValue";
    private const string BGM_VOLUME_KEY = "BGMVolumeValue";
    private const string SFX_VOLUME_KEY = "SFXVolumeValue";

    [Header("Master")]
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private TMP_Text _masterText;
    [SerializeField] private Button _masterMuteButton;

    [Header("BGM")]
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private TMP_Text _bgmText;
    [SerializeField] private Button _bgmMuteButton;

    [Header("SFX")]
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private TMP_Text _sfxText;
    [SerializeField] private Button _sfxMuteButton;

    [Header("Confirm Popup")]
    [SerializeField] private ConfirmPopupUI _confirmPopup;

    [Header("Buttons")]
    [SerializeField] private Button _applyButton;
    [SerializeField] private Button _cancelButton;

    [SerializeField] private GameObject _settingPopup;

    private float _currentMasterVolume;
    private float _currentBGMVolume;
    private float _currentSFXVolume;

    private float _selectedMasterVolume;
    private float _selectedBGMVolume;
    private float _selectedSFXVolume;

    private float _prevMasterVolume = 1f;
    private float _prevBGMVolume = 1f;
    private float _prevSFXVolume = 1f;

    private bool _isApplyMode;
    void Awake()
    {

        

        Init();
    }
    private void Start()
    {
        _masterMuteButton.onClick.AddListener(OnClickMasterMute);
        _bgmMuteButton.onClick.AddListener(OnClickBGMMute);
        _sfxMuteButton.onClick.AddListener(OnClickSFXMute);

    }

    public void Init()
    {
        _masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        _bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        _sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);

        _applyButton.onClick.AddListener(OnClickApplyButton);
        _cancelButton.onClick.AddListener(OnClickCancelButton);


        InitSlider(_masterSlider);
        InitSlider(_bgmSlider);
        InitSlider(_sfxSlider);

        _currentMasterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        _currentBGMVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
        _currentSFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        _selectedMasterVolume = _currentMasterVolume;
        _selectedBGMVolume = _currentBGMVolume;
        _selectedSFXVolume = _currentSFXVolume;

        ApplySliderValuesWithoutNotify();
        ApplySelectedVolume();
        RefreshAllTexts();
    }

    private void InitSlider(Slider slider)
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void OnMasterSliderChanged(float value)
    {
        _selectedMasterVolume = value;
        SoundEvents.RaiseSetMasterVolumeEvent(value);
        RefreshText(_masterText, value);
    }

    private void OnBGMSliderChanged(float value)
    {
        _selectedBGMVolume = value;
        SoundEvents.RaiseSetBGMVolumeEvent(value);
        RefreshText(_bgmText, value);
    }

    private void OnSFXSliderChanged(float value)
    {
        _selectedSFXVolume = value;
        SoundEvents.RaiseSetSFXVolumeEvent(value);
        RefreshText(_sfxText, value);
    }

    private void OnClickMasterMute()
    {
        ToggleMute(_masterSlider, ref _prevMasterVolume);
    }

    private void OnClickBGMMute()
    {
        ToggleMute(_bgmSlider, ref _prevBGMVolume);
    }

    private void OnClickSFXMute()
    {
        ToggleMute(_sfxSlider, ref _prevSFXVolume);
    }

    private void ToggleMute(Slider slider, ref float prevVolume)
    {
        if (slider.value > 0f)
        {
            prevVolume = slider.value;
            slider.value = 0f;
        }
        else
        {
            slider.value = Mathf.Clamp(prevVolume, 0.01f, 1f);
        }
    }

    private void TryOpenConfirmPopup()
    {
        Debug.Log($"사운드 변경사항 여부 : {HasChanged()}");

        if (HasChanged())
        {
            if (_confirmPopup != null)
            {
                _confirmPopup.Open(
                    onApply: () =>
                    {
                        if (_isApplyMode)
                            ApplySoundSetting();
                        else
                            CancelSoundSetting();

                        if (_settingPopup != null)
                            _settingPopup.SetActive(false);
                    },
                    onDiscard: () =>
                    {
                        // 컨펌팝업에서 취소 누르면 그냥 컨펌팝업만 닫힘
                    }
                );
            }
            else
            {
                Debug.LogError("ConfirmPopupUI가 연결되지 않음");
            }
        }
        else
        {
            if (_settingPopup != null)
                _settingPopup.SetActive(false);
        }
    }

    public void ApplySoundSetting()
    {
        _currentMasterVolume = _selectedMasterVolume;
        _currentBGMVolume = _selectedBGMVolume;
        _currentSFXVolume = _selectedSFXVolume;

        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, _currentMasterVolume);
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, _currentBGMVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, _currentSFXVolume);
        PlayerPrefs.Save();

        ApplySelectedVolume();
    }

    public void CancelSoundSetting()
    {
        _selectedMasterVolume = _currentMasterVolume;
        _selectedBGMVolume = _currentBGMVolume;
        _selectedSFXVolume = _currentSFXVolume;

        ApplySliderValuesWithoutNotify();
        ApplySelectedVolume();
        RefreshAllTexts();
    }

    private bool HasChanged()
    {
        return !Mathf.Approximately(_currentMasterVolume, _selectedMasterVolume)
            || !Mathf.Approximately(_currentBGMVolume, _selectedBGMVolume)
            || !Mathf.Approximately(_currentSFXVolume, _selectedSFXVolume);
    }

    private void ApplySliderValuesWithoutNotify()
    {
        _masterSlider.SetValueWithoutNotify(_selectedMasterVolume);
        _bgmSlider.SetValueWithoutNotify(_selectedBGMVolume);
        _sfxSlider.SetValueWithoutNotify(_selectedSFXVolume);
    }

    private void ApplySelectedVolume()
    {
        SoundEvents.RaiseSetMasterVolumeEvent(_selectedMasterVolume);
        SoundEvents.RaiseSetBGMVolumeEvent(_selectedBGMVolume);
        SoundEvents.RaiseSetSFXVolumeEvent(_selectedSFXVolume);
    }

    private void RefreshAllTexts()
    {
        RefreshText(_masterText, _selectedMasterVolume);
        RefreshText(_bgmText, _selectedBGMVolume);
        RefreshText(_sfxText, _selectedSFXVolume);
    }

    private void RefreshText(TMP_Text text, float value)
    {
        if (text == null) return;

        text.text = $"{Mathf.RoundToInt(value * 100)}";
    }
    private void OnClickApplyButton()
    {
        _isApplyMode = true;
        TryOpenConfirmPopup();
    }

    private void OnClickCancelButton()
    {
        _isApplyMode = false;
        TryOpenConfirmPopup();
    }
}