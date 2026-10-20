using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class DisplayTabUI : MonoBehaviour
{
    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private TMP_Dropdown _screenModeDropdown;

    [Header("Buttons")]
    [SerializeField] private Button _applyButton;
    [SerializeField] private Button _cancelButton;

    [Header("V_SyncToggle")]
    [SerializeField] private Toggle _vSyncToggle;
    [SerializeField] private TMP_Text _vSyncText;

    private const string ResolutionIndexKey = "ResolutionIndex";
    private const string ScreenModeIndexKey = "ScreenModeIndex";
    private const string VSyncKey = "VSync";

    [Header("Popup")]
    [SerializeField] private GameObject _settingPopup;
    [SerializeField] private ConfirmPopupUI _confirmPopupUI;

    private int _currentResolutionIndex;
    private int _selectedResolutionIndex;

    private int _currentScreenModeIndex;
    private int _selectedScreenModeIndex;

    private bool _currentVSync;
    private bool _selectedVSync;

    //사용자 지원 해상도
    private List<Resolution> _resolutions = new();


    //대표 해상도 필터링 목록
    private readonly HashSet<Vector2Int> _allowedResolutions = new()
    {
    new Vector2Int(1280, 720),
    new Vector2Int(1600, 900),
    new Vector2Int(1920, 1080),
    new Vector2Int(2560, 1440),
    new Vector2Int(3840, 2160)
    };

    
    private readonly List<FullScreenMode> _screenModes = new()
    {
        FullScreenMode.FullScreenWindow,
        FullScreenMode.Windowed,
        FullScreenMode.ExclusiveFullScreen
    };

    private void Awake()
    {
        InitResolutionDropdown();
        InitScreenModeDropdown();

        _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        _screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);

        _applyButton.onClick.AddListener(OnClickApplyButton);
        _cancelButton.onClick.AddListener(OnClickCancelButton);
    }

    private void OnEnable()
    {
        InitVSync();
    }
    private void OnClickApplyButton()
    {
        TryOpenConfirmPopup();
    }

    private void OnClickCancelButton()
    {
        TryOpenConfirmPopup();
    }

    private void TryOpenConfirmPopup()
    {
        Debug.Log($"변경사항 여부: {HasPendingChanges()}");

        if (HasPendingChanges())
        {
            Debug.Log("컨펌 팝업 열기");

            if (_confirmPopupUI != null)
            {
                _confirmPopupUI.Open(
                    onApply: () =>
                    {
                        ApplyDisplaySetting();

                        if (_settingPopup != null)
                            _settingPopup.SetActive(false);
                    },
                    onDiscard: () =>
                    {
                        CancelDisplaySetting();

                        if (_settingPopup != null)
                            _settingPopup.SetActive(false);
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
            Debug.Log("변경사항 없음 - 설정창 닫기");

            if (_settingPopup != null)
                _settingPopup.SetActive(false);
            else
                Debug.LogError("Setting Popup이 연결되지 않음");
        }
    }

    private void InitResolutionDropdown()
    {
        _resolutionDropdown.ClearOptions();

        _resolutions.Clear();

        List<string> options = new();

        Resolution[] supportedResolutions = Screen.resolutions;

        for (int i = 0; i < supportedResolutions.Length; i++)
        {
            Resolution resolution = supportedResolutions[i];

            Vector2Int size = new Vector2Int(resolution.width, resolution.height);

            // 대표 해상도만 허용
            if (!_allowedResolutions.Contains(size))
                continue;

            _resolutions.Add(resolution);

            options.Add(
                $"{resolution.width} x {resolution.height} ({resolution.refreshRateRatio.value:0}Hz)"
            );

            // 현재 사용중인 해상도 찾기
            if (Screen.width == resolution.width &&
                Screen.height == resolution.height)
            {
                _currentResolutionIndex = options.Count - 1;
            }
        }

        // 해상도 목록이 비어있을 경우
        if (_resolutions.Count == 0)
        {
            Resolution current = Screen.currentResolution;

            _resolutions.Add(current);

            options.Add(
                $"{current.width} x {current.height} ({current.refreshRateRatio.value:0}Hz)"
            );

            _currentResolutionIndex = 0;
        }

        // 저장된 해상도 인덱스 불러오기
        _currentResolutionIndex = PlayerPrefs.GetInt(ResolutionIndexKey,_currentResolutionIndex);

        // 인덱스 범위 보호
        _currentResolutionIndex = Mathf.Clamp(_currentResolutionIndex,0,_resolutions.Count - 1);

        _selectedResolutionIndex = _currentResolutionIndex;

        _resolutionDropdown.AddOptions(options);

        _resolutionDropdown.SetValueWithoutNotify(_selectedResolutionIndex);

        _resolutionDropdown.RefreshShownValue();
    }

    private void InitScreenModeDropdown()
    {
        _screenModeDropdown.ClearOptions();

        List<string> options = new()
    {
        "테두리 없는 창모드",
        "창모드",
        "전체화면"
    };

        _screenModeDropdown.AddOptions(options);

        // 저장된 창모드 불러오기
        _currentScreenModeIndex = PlayerPrefs.GetInt(
            ScreenModeIndexKey,
            GetCurrentScreenModeIndex()
        );

        // 인덱스 범위 보호
        _currentScreenModeIndex = Mathf.Clamp(
            _currentScreenModeIndex,
            0,
            _screenModes.Count - 1
        );

        _selectedScreenModeIndex = _currentScreenModeIndex;

        _screenModeDropdown.SetValueWithoutNotify(
            _selectedScreenModeIndex
        );

        _screenModeDropdown.RefreshShownValue();
    }
    private void InitVSync()
    {
        // 저장된 값 불러오기 (기본값 : 켜짐)
        _currentVSync = PlayerPrefs.GetInt(VSyncKey, 1) == 1;

        _selectedVSync = _currentVSync;

        // 실제 적용
        ApplyVSync(_currentVSync);

        // UI 반영
        _vSyncToggle.SetIsOnWithoutNotify(_selectedVSync);

        // 텍스트 초기화
        _vSyncText.text = _selectedVSync ? "켜기" : "끄기";

        // 이벤트 등록
        _vSyncToggle.onValueChanged.AddListener(OnChangedVSync);
    }

    private void OnChangedVSync(bool isOn)
    {
        _selectedVSync = isOn;
        _vSyncText.text = isOn ? "켜기" : "끄기";
    }


    private int GetCurrentScreenModeIndex()
    {
        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
            return 0;

        if (Screen.fullScreenMode == FullScreenMode.Windowed)
            return 1;

        if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
            return 2;

        return 0;
    }
    private void OnResolutionChanged(int index)
    {
        _selectedResolutionIndex = index;
        Debug.Log($"선택 해상도 인덱스: {_selectedResolutionIndex}");
    }

    private void OnScreenModeChanged(int index)
    {
        _selectedScreenModeIndex = index;
        Debug.Log($"선택 화면모드 인덱스: {_selectedScreenModeIndex}");
    }

    public void ApplyDisplaySetting()
    {
        Resolution resolution = _resolutions[_selectedResolutionIndex];
        FullScreenMode screenMode = _screenModes[_selectedScreenModeIndex];

        Screen.SetResolution(
            resolution.width,
            resolution.height,
            screenMode,
            resolution.refreshRateRatio
        );

       
        ApplyVSync(_selectedVSync);

        _currentResolutionIndex = _selectedResolutionIndex;
        _currentScreenModeIndex = _selectedScreenModeIndex;
        _currentVSync = _selectedVSync;

        
        PlayerPrefs.SetInt(ResolutionIndexKey, _currentResolutionIndex);
        PlayerPrefs.SetInt(ScreenModeIndexKey, _currentScreenModeIndex);
        PlayerPrefs.SetInt(VSyncKey, _currentVSync ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"화면 설정 적용: {resolution.width} x {resolution.height} / {screenMode} / VSync: {_currentVSync}");
    }

    public void CancelDisplaySetting()
    {
        _selectedResolutionIndex = _currentResolutionIndex;
        _selectedScreenModeIndex = _currentScreenModeIndex;
        _selectedVSync = _currentVSync;

        _resolutionDropdown.SetValueWithoutNotify(_selectedResolutionIndex);
        _screenModeDropdown.SetValueWithoutNotify(_selectedScreenModeIndex);
        _vSyncToggle.SetIsOnWithoutNotify(_selectedVSync);

        _resolutionDropdown.RefreshShownValue();
        _screenModeDropdown.RefreshShownValue();

        Debug.Log("화면 설정 변경 취소");
    }

    public bool HasPendingChanges()
    {
        return _currentResolutionIndex != _selectedResolutionIndex ||
               _currentScreenModeIndex != _selectedScreenModeIndex ||
               _currentVSync != _selectedVSync;
    }

    private void ApplyVSync(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;

        if (isOn)
            Application.targetFrameRate = -1;
        else
            Application.targetFrameRate = 144;
    }
}