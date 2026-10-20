using UnityEngine;
using UnityEngine.UI;

public class PlayerStaminaUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider _staminaSlider;
    [SerializeField] private float _smoothSpeed = 10f;

    private float _targetStamina;



    private void Start()
    {
        _staminaSlider.value = 100;
        _targetStamina = 100;
    }

    private void OnEnable()
    {
        UIEvents.OnStaminaChanged += OnMPChanged;
    }

    private void OnDisable()
    {
        UIEvents.OnStaminaChanged -= OnMPChanged;
    }

    private void Update()
    {
        UpdateMPBar();
    }

    private void OnMPChanged(float currentMP, float maxMP)
    {
        _staminaSlider.maxValue = maxMP;
        _targetStamina = currentMP;
    }

    private void UpdateMPBar()
    {
        _staminaSlider.value = Mathf.Lerp(
            _staminaSlider.value,
            _targetStamina,
            _smoothSpeed * Time.deltaTime
        );

        if (Mathf.Abs(_staminaSlider.value - _targetStamina) < 0.1f)
        {
            _staminaSlider.value = _targetStamina;
        }
    }
}