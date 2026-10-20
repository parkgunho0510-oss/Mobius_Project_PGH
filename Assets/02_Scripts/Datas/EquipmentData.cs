using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "Scriptable Objects/EquipmentData")]
public class EquipmentData : ItemData
{
    [Header("Equipment Specific")]
    public EquipmentType EquipmentType;
    public GradeType GradeType;
    public float ATK;
    public float DEF;
    public float STAMINA;
    public float HP;
    public float LUCK;

    public string AnimDataID; // CSV Header - AnimDataID 

    [SerializeField] private EquipmentAnimData _animData;
    public EquipmentAnimData AnimData => _animData;

    [Header("Asset Paths (CSV Matching)")]

    public GameObject WeaponPrefab;
    //public AnimatorOverrideController WeaponAnimOverride;

    public override void LinkData(DataManager dataManager)
    {
        // Load from Resources_Animations_ANIM_LON_002) 
        if (!string.IsNullOrEmpty(AnimDataID))
        {
            _animData = Resources.Load<EquipmentAnimData>($"Animations/{AnimDataID}");
        }
    }

    public override InventoryItem CreateItem()
    {
        return new EquipmentItem(this);
    }
}
