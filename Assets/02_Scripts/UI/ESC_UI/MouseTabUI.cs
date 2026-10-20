using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class MouseTabUI : MonoBehaviour
{
    private const string MouseRotateSpeedKey = "MouseRotateSpeed";

    [Header("UI")]
    [SerializeField] private Slider _rotateSpeedSlider;
    [SerializeField] private TMP_Text _rotateSpeedText;
    [SerializeField] private Button _minusButton;
    [SerializeField] private Button _plusButton;

    [Header("Popup")]
    [SerializeField] private ConfirmPopupUI _confirmPopupUI;
    [SerializeField] private GameObject _settingPopup;

    [Header("Value")]
    [SerializeField] private float _defaultValue = 30f;
    [SerializeField] private float _minValue = 1f;
    [SerializeField] private float _maxValue = 100f;
    [SerializeField] private float _step = 1f;

    [Header("Setting Buttons")]
    [SerializeField] private Button _applyButton;
    [SerializeField] private Button _cancelButton;

    [Header("Cinemachine")]
    [SerializeField] private CinemachineInputAxisController _inputAxisController;

    private float _currentRotateSpeed;
    private float _selectedRotateSpeed;

    private bool _isApplyMode;

    private float[] _baseGains;

    private void Awake()
    {
        if (_inputAxisController == null)
            _inputAxisController = FindFirstObjectByType<CinemachineInputAxisController>();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        _minValue = Mathf.Max(_minValue, 1f);

        _rotateSpeedSlider.minValue = _minValue;
        _rotateSpeedSlider.maxValue = _maxValue;
        _rotateSpeedSlider.wholeNumbers = true;

        _currentRotateSpeed = Mathf.Clamp(
            PlayerPrefs.GetFloat(MouseRotateSpeedKey, _defaultValue),
            _minValue,
            _maxValue
        );

        _selectedRotateSpeed = _currentRotateSpeed;

        _rotateSpeedSlider.SetValueWithoutNotify(_selectedRotateSpeed);

        RefreshText(_selectedRotateSpeed);

        CacheBaseGains();
        ApplyCinemachineGain(_currentRotateSpeed);

        _rotateSpeedSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        _minusButton.onClick.RemoveListener(OnClickMinus);
        _plusButton.onClick.RemoveListener(OnClickPlus);
        _applyButton.onClick.RemoveListener(OnClickApplyButton);
        _cancelButton.onClick.RemoveListener(OnClickCancelButton);

        _rotateSpeedSlider.onValueChanged.AddListener(OnSliderValueChanged);
        _minusButton.onClick.AddListener(OnClickMinus);
        _plusButton.onClick.AddListener(OnClickPlus);
        _applyButton.onClick.AddListener(OnClickApplyButton);
        _cancelButton.onClick.AddListener(OnClickCancelButton);
    }
    private void OnSliderValueChanged(float value)
    {
        value = Mathf.Clamp(value, _minValue, _maxValue);

        if (!Mathf.Approximately(_rotateSpeedSlider.value, value))
        {
            _rotateSpeedSlider.SetValueWithoutNotify(value);
        }

        _selectedRotateSpeed = value;
        RefreshText(_selectedRotateSpeed);
    }
    private void OnClickMinus()
    {
        _rotateSpeedSlider.value = Mathf.Clamp(
            _rotateSpeedSlider.value - _step,
            _minValue,
            _maxValue
        );
    }

    private void OnClickPlus()
    {
        _rotateSpeedSlider.value = Mathf.Clamp(
            _rotateSpeedSlider.value + _step,
            _minValue,
            _maxValue
        );
    }

    public bool HasChanged()
    {
        return !Mathf.Approximately(_currentRotateSpeed, _selectedRotateSpeed);
    }

    public void ApplyMouseSetting()
    {
        _currentRotateSpeed = Mathf.Clamp(
            _selectedRotateSpeed,
            _minValue,
            _maxValue
        );

        _selectedRotateSpeed = _currentRotateSpeed;

        PlayerPrefs.SetFloat(MouseRotateSpeedKey, _currentRotateSpeed);
        PlayerPrefs.Save();

        ApplyCinemachineGain(_currentRotateSpeed);

        RefreshText(_currentRotateSpeed);

        Debug.Log($"마우스 감도 저장 및 적용 : {_currentRotateSpeed}");
    }

    public void CancelMouseSetting()
    {
        _selectedRotateSpeed = _currentRotateSpeed;

        _rotateSpeedSlider.SetValueWithoutNotify(_selectedRotateSpeed);
        RefreshText(_selectedRotateSpeed);

        Debug.Log("마우스 감도 변경 취소");
    }

    private void CacheBaseGains()
    {
        if (_inputAxisController == null)
            _inputAxisController = FindFirstObjectByType<CinemachineInputAxisController>();

        if (_inputAxisController == null)
        {
            Debug.Log("CinemachineInputAxisController 없음 - 현재 씬에서는 적용 스킵");
            return;
        }

        int count = _inputAxisController.Controllers.Count;
        _baseGains = new float[count];

        for (int i = 0; i < count; i++)
        {
            string axisName = _inputAxisController.Controllers[i].Name;

            if (axisName == "Look Orbit X" || axisName == "Look Orbit Y")
            {
                _baseGains[i] = _inputAxisController.Controllers[i].Input.Gain;
                Debug.Log($"기본 Gain 저장 / index:{i} / name:{axisName} / gain:{_baseGains[i]}");
            }
        }
    }

    private void ApplyCinemachineGain(float value)
    {
        if (_inputAxisController == null)
            _inputAxisController = FindFirstObjectByType<CinemachineInputAxisController>();

        if (_inputAxisController == null)
            return;

        if (_baseGains == null || _baseGains.Length != _inputAxisController.Controllers.Count)
            CacheBaseGains();

        float ratio = value / _defaultValue;

        Debug.Log($"감도 적용 시작 / value:{value} / default:{_defaultValue} / ratio:{ratio}");

        for (int i = 0; i < _inputAxisController.Controllers.Count; i++)
        {
            string axisName = _inputAxisController.Controllers[i].Name;

            if (!axisName.Contains("Look Orbit"))
                continue;

            float newGain = _baseGains[i] * ratio;

            _inputAxisController.Controllers[i].Input.Gain = newGain;

            Debug.Log($"Gain 적용 / index:{i} / name:{axisName} / base:{_baseGains[i]} / ratio:{ratio} / result:{newGain}");
        }
    }

    private void RefreshText(float value)
    {
        _rotateSpeedText.text = value.ToString("0");
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

    private void TryOpenConfirmPopup()
    {
        Debug.Log($"변경사항 여부 : {HasChanged()}");

        if (HasChanged())
        {
            if (_confirmPopupUI != null)
            {
                _confirmPopupUI.Open(
                    onApply: () =>
                    {
                        if (_isApplyMode)
                            ApplyMouseSetting();
                        else
                            CancelMouseSetting();

                        if (_settingPopup != null)
                            _settingPopup.SetActive(false);
                    },
                    onDiscard: () =>
                    {
                        // 컨펌팝업에서 취소 누르면 그냥 팝업만 닫힘
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

    public void OnClickConfirmOk()
    {
        if (_isApplyMode)
            ApplyMouseSetting();
        else
            CancelMouseSetting();

        if (_confirmPopupUI != null)
            _confirmPopupUI.Close();

        if (_settingPopup != null)
            _settingPopup.SetActive(false);
    }

    public void OnClickConfirmCancel()
    {
        if (_confirmPopupUI != null)
            _confirmPopupUI.Close();
    }
}