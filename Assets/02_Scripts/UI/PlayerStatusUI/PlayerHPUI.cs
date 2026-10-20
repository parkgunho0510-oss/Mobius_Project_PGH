using UnityEngine;
using UnityEngine.UI;

public class PlayerHPUI : MonoBehaviour
{
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private float _smoothSpeed = 10f;

    private float _targetHP;

    private void Start()
    {
      
    }

    private void OnEnable()
    {
        UIEvents.OnHPChanged += OnHPChanged;
    }

    private void OnDisable()
    {
        UIEvents.OnHPChanged -= OnHPChanged;
    }

    private void Update()
    {
        UpdateHPBar();
    }

    private void OnHPChanged(float currentHP, float maxHP)
    {
        _hpSlider.maxValue = maxHP;
        _targetHP = currentHP;
    }

    private void UpdateHPBar()
    {
        _hpSlider.value = Mathf.Lerp(
            _hpSlider.value,
            _targetHP,
            _smoothSpeed * Time.deltaTime
        );

        if (Mathf.Abs(_hpSlider.value - _targetHP) < 0.1f)
        {
            _hpSlider.value = _targetHP;
        }
    }
}