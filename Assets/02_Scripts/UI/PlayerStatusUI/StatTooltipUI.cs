using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class StatTooltipUI : MonoBehaviour
{
    [Header("Tooltip Panel")]
    [SerializeField] private GameObject _tooltipPanel;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _contentText;
    [SerializeField] private TMP_Text _valueText;

    [Header("Stat Icons")]
    [SerializeField] private Image _hpIcon;
    [SerializeField] private Image _staminaIcon;
    [SerializeField] private Image _atkIcon;
    [SerializeField] private Image _defIcon;
    [SerializeField] private Image _luckIcon;

    private void Awake()
    {
        Hide();
    }

    private void Update()
    {
        if (IsPointerOver(_hpIcon))
        {
            Show("체력", "캐릭터의 최대 생명력입니다.", "레벨업 시 최대 체력 +25 \n스탯 레벨업은 로비(마을)에서 가능합니다.");
        }
        else if (IsPointerOver(_staminaIcon))
        {
            Show("스태미나", "회피나 행동에 사용되는 자원입니다.", "레벨업 시 최대 스태미나 +10 \n스탯 레벨업은 로비(마을)에서 가능합니다.");
        }
        else if (IsPointerOver(_atkIcon))
        {
            Show("공격력", "적에게 주는 기본 피해량입니다.", "레벨업 시 공격력 +5 \n스탯 레벨업은 로비(마을)에서 가능합니다.");
        }
        else if (IsPointerOver(_defIcon))
        {
            Show("방어력", "받는 피해를 줄여주는 수치입니다.", "레벨업 시 방어력 +2 \n스탯 레벨업은 로비(마을)에서 가능합니다.");
        }
        else if (IsPointerOver(_luckIcon))
        {
            Show("행운", "아이템 획득이나 확률에 영향을 줍니다.", "LUCK스탯은 아이템으로만 증가됩니다.");
        }
        else
        {
            Hide();
        }
    }

    private bool IsPointerOver(Image icon)
    {
        if (icon == null || Mouse.current == null) 
            return false;

        RectTransform rect = icon.rectTransform;

        return RectTransformUtility.RectangleContainsScreenPoint(
            rect,
            Mouse.current.position.ReadValue(),
            null
        );
    }

    private void Show(string title, string content, string value)
    {
        _titleText.text = title;
        _contentText.text = content;
        _valueText.text = value;

        _tooltipPanel.SetActive(true);
    }

    private void Hide()
    {
        _tooltipPanel.SetActive(false);
    }
}