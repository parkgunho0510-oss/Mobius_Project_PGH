using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHPUI : MonoBehaviour
{
    [SerializeField] private BossBase _boss;
    [SerializeField] private PlayerStatus _player;
    [SerializeField] private GameObject _uiCanvas;

    [SerializeField] private TMP_Text _bossNameText;
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private float _smoothSpeed = 5f;

    private float _targetHP;

    private void Start()
    {
        InitUI();
    }

    private void InitUI()
    {
        if (_boss == null)
        {
            Debug.LogWarning("BossHPUI : Boss 없음");
            return;
        }

        if (_boss.Data == null)
        {
            Debug.LogWarning("BossHPUI : Boss Data 없음");
            return;
        }

        _bossNameText.text = _boss.Data.MonsterName;

        _hpSlider.minValue = 0f;
        _hpSlider.maxValue = _boss.MaxHp;
        _hpSlider.value = _boss.CurrentHp;

        _targetHP = _boss.CurrentHp;
    }

    private void Update()
    {
        if (_boss == null || _boss.Data == null) return;

        if (_uiCanvas != null && _uiCanvas.activeSelf)
        {
            // 보스 사망 시 UI 끄기
            if (_boss.CurrentHp <= 0f)
            {
                _uiCanvas.SetActive(false);
                return;
            }

        
        }

        _targetHP = _boss.CurrentHp;

        _hpSlider.value = Mathf.Lerp(
            _hpSlider.value,
            _targetHP,
            Time.deltaTime * _smoothSpeed
        );

        if (Mathf.Abs(_hpSlider.value - _targetHP) < 0.1f)
        {
            _hpSlider.value = _targetHP;
        }
    }
}