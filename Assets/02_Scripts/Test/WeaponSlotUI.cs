//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class WeaponSlotUI : MonoBehaviour
//{
//    [Header("UI References")]
//    [SerializeField] Image _iconImage;
//    [SerializeField] Image _highlightBorder;   
//    [SerializeField] Image _emptyOverlay;      
//    [SerializeField] TextMeshProUGUI _weaponNameText;

//    [SerializeField] private EquipmentData _assignedData;
//    public EquipmentData AssignedData => _assignedData;
//    public bool IsEmpty => _assignedData == null;

//    void Start() => Refresh();

//    void Refresh()
//    {
//        bool has = _assignedData != null;

//        if (_iconImage != null)
//        {
//            _iconImage.enabled = has;
//            _iconImage.sprite = has ? _assignedData.Icon : null;
//        }

//        if (_emptyOverlay != null) _emptyOverlay.enabled = !has;
//        if (_weaponNameText != null) _weaponNameText.text = has ? _assignedData.ItemName : string.Empty;
//    }

//    public void AssignWeapon(EquipmentData data)
//    {
//        _assignedData = data;
//        Refresh();
//    }

//    public void ClearWeapon()
//    {
//        _assignedData = null;
//        Refresh();
//    }

 
//    public void SetHighlight(bool isActive)
//    {
//        if (_highlightBorder != null)
//            _highlightBorder.enabled = isActive;
//    }


//}