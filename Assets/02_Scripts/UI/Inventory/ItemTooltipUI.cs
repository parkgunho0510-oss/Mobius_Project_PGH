using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;



    /// <summary> 슬롯 내의 아이템 아이콘에 마우스를 올렸을 때 보이는 툴팁 </summary>
    public class ItemTooltipUI : MonoBehaviour
    {

    [SerializeField] private Image _iconImage;

    /***********************************************************************
    *                           Inspector Option Fields
    ***********************************************************************/
    #region .
    [SerializeField] private TMP_Text _titleText;   // 아이템 이름 텍스트

    [SerializeField] private TMP_Text _contentText; // 아이템 설명 텍스트


    [SerializeField] private TextMeshProUGUI[] _statTexts;

    [SerializeField] private TMP_Text _gradeText;

    #endregion
    /***********************************************************************
    *                               Private Fields
    ***********************************************************************/
    #region .
    private RectTransform _rt;
    private CanvasScaler _canvasScaler;
    private ItemData _equippedData;

        private static readonly Vector2 LeftTop = new Vector2(0f, 1f);
        private static readonly Vector2 LeftBottom = new Vector2(0f, 0f);
        private static readonly Vector2 RightTop = new Vector2(1f, 1f);
        private static readonly Vector2 RightBottom = new Vector2(1f, 0f);

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Awake()
        {
            Init();
            Hide();
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        private void Init()
        {
            TryGetComponent(out _rt);
            _rt.pivot = LeftTop;
            _canvasScaler = GetComponentInParent<CanvasScaler>();

            DisableAllChildrenRaycastTarget(transform);
        }

        /// <summary> 모든 자식 UI에 레이캐스트 타겟 해제 </summary>
        private void DisableAllChildrenRaycastTarget(Transform tr)
        {
            // 본인이 Graphic(UI)를 상속하면 레이캐스트 타겟 해제
            tr.TryGetComponent(out Graphic gr);
            if (gr != null)
                gr.raycastTarget = false;

            // 자식이 없으면 종료
            int childCount = tr.childCount;
            if (childCount == 0) return;

            for (int i = 0; i < childCount; i++)
            {
                DisableAllChildrenRaycastTarget(tr.GetChild(i));
            }
        }

        #endregion
        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .
        /// <summary> 툴팁 UI에 아이템 정보 등록 </summary>
        public void SetItemInfo(ItemData data)
        {
        _iconImage.sprite = data.Icon;
        _titleText.text = data.ItemName;
        _contentText.text = data.Description;
        _gradeText.gameObject.SetActive(false);

        if (data is EquipmentData equip)
        {
            _gradeText.gameObject.SetActive(true);

            _gradeText.text = GetGradeText(equip.GradeType);
            _gradeText.color = GetGradeColor(equip.GradeType);
        }
    }

    private Color GetGradeColor(GradeType grade)
    {
        switch (grade)
        {
            case GradeType.COMMON:
                return new Color(0.65f, 0.65f, 0.65f);

            case GradeType.RARE:
                return Color.red;

            case GradeType.EPIC:
                return new Color(0.6f, 0f, 1f);

            default:
                return Color.white;
        }
    }
    private string GetGradeText(GradeType grade)
    {
        switch (grade)
        {
            case GradeType.COMMON:
                return "(COMMON)";

            case GradeType.RARE:
                return "(RARE)";

            case GradeType.EPIC:
                return "(EPIC)";

            default:
                return "";
        }
    }


    public void SetEquippedData(ItemData equipped)
    {
        _equippedData = equipped;
    }

    public void SetStatInfo(ItemData data)
    {
        HideAllStatTexts();

        if (data == null)
            return;

        int index = 0;

        if (data.Type == ItemType.CONSUMABLE)
        {
            ConsumableData potion = data as ConsumableData;

            if (potion == null)
                return;

            float diff = GetValueDiff(potion);

            AddStatText(
                ref index,
                $"회복량 : {potion.Value}{GetDiffText(diff)}"
            );
        }
        else if (data.Type == ItemType.EQUIPMENT)
        {
            EquipmentData equip = data as EquipmentData;
            if (equip == null)
                return;
            float atkDiff = GetATKDiff(equip);
            if (!(equip.ATK == 0 && atkDiff == 0))
            {
                AddStatText(ref index,
                    $"공격력 : {equip.ATK}{GetDiffText(atkDiff)}");
            }

            float defDiff = GetDEFDiff(equip);
            if (!(equip.DEF == 0 && defDiff == 0))
            {
                AddStatText(ref index,
                    $"방어력 : {equip.DEF}{GetDiffText(defDiff)}");
            }

            float hpDiff = GetHPDiff(equip);
            if (!(equip.HP == 0 && hpDiff == 0))
            {
                AddStatText(ref index,
                    $"체력 : {equip.HP}{GetDiffText(hpDiff)}");
            }

            float staminaDiff = GetStaminaDiff(equip);
            if (!(equip.STAMINA == 0 && staminaDiff == 0))
            {
                AddStatText(ref index,
                    $"기력 : {equip.STAMINA}{GetDiffText(staminaDiff)}");
            }

            float luckDiff = GetLuckDiff(equip);
            if (!(equip.LUCK == 0 && luckDiff == 0))
            {
                AddStatText(ref index,
                    $"행운 : {equip.LUCK}{GetDiffText(luckDiff)}");
            }
        }
    }
    private string GetDiffText(float diff)
    {
        if (diff > 0)
            return $" <color=green>(+{diff})</color>";
        else
            return $" <color=red>({diff})</color>";
    }

    private float GetATKDiff(EquipmentData weapon)
    {
        // 장착 무기 있으면 비교
        if (_equippedData is EquipmentData eqWeapon)
        {
            return weapon.ATK - eqWeapon.ATK;
        }

        // 장착 무기 없으면
        return weapon.ATK;
    }

    private float GetDEFDiff(EquipmentData armor)
    {
        if (_equippedData is EquipmentData eqArmor)
        {
            return armor.DEF - eqArmor.DEF;
        }

        return armor.DEF;
    }

    private float GetValueDiff(ConsumableData potion)
    {
        if (_equippedData is ConsumableData eqPotion)
        {
            return potion.Value - eqPotion.Value;
        }

        return potion.Value;
    }
    private float GetHPDiff(EquipmentData equip)
    {
        if (_equippedData is EquipmentData eqEquip)
            return equip.HP - eqEquip.HP;

        return equip.HP;
    }

    private float GetStaminaDiff(EquipmentData equip)
    {
        if (_equippedData is EquipmentData eqEquip)
            return equip.STAMINA - eqEquip.STAMINA;

        return equip.STAMINA;
    }

    private float GetLuckDiff(EquipmentData equip)
    {
        if (_equippedData is EquipmentData eqEquip)
            return equip.LUCK - eqEquip.LUCK;

        return equip.LUCK;
    }
    private void HideAllStatTexts()
    {
        foreach (var text in _statTexts)
        {
            text.gameObject.SetActive(false);
            text.text = "";
        }
    }

    private void AddStatText(ref int index, string content)
    {
        if (index >= _statTexts.Length)
            return;

        _statTexts[index].gameObject.SetActive(true);
        _statTexts[index].text = content;

        index++;
    }

    public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        #endregion
    }
