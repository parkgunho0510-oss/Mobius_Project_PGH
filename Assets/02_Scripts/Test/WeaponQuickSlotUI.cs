using UnityEngine;
using UnityEngine.UI;

public class WeaponQuickSlotUI : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private CurrentUsingItems _currentUsingItems;

    [Header("Weapon Icon")]
    [SerializeField] private Image _mainWeaponIcon;
    [SerializeField] private Image _subWeaponIcon;

    private void OnEnable()
    {
        UIEvents.OnWeaponSwitched += UpdateWeaponUI;
        UIEvents.OnWeaponEquipped += UpdateWeaponUI;
    }

    private void OnDisable()
    {
        UIEvents.OnWeaponSwitched -= UpdateWeaponUI;
        UIEvents.OnWeaponEquipped -= UpdateWeaponUI;
    }

    private void Start()
    {
        if (_currentUsingItems == null) _currentUsingItems = FindFirstObjectByType<CurrentUsingItems>();
        Refresh();
    }

    private void UpdateWeaponUI(EquipmentData currentWeapon, EquipmentData subWeapon)
    {
        SetIcon(_mainWeaponIcon, currentWeapon);
        SetIcon(_subWeaponIcon, subWeapon);
    }

    private void Refresh()
    {
        if (_currentUsingItems == null)
            return;

        EquipmentData current = _currentUsingItems.CurrentWeapon;
        EquipmentData sub = _currentUsingItems.GetSubWeapon();

        UpdateWeaponUI(current, sub);
    }

    private void SetIcon(Image icon, EquipmentData weaponData)
    {
        if (icon == null)
            return;

        if (weaponData == null || weaponData.Icon == null)
        {
            icon.enabled = false;
            icon.sprite = null;
            return;
        }

        icon.enabled = true;
        icon.sprite = weaponData.Icon;
    }
}