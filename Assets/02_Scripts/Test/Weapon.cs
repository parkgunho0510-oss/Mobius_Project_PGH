using UnityEngine;
using System.Collections.Generic;


public class Weapon : MonoBehaviour
{
    [SerializeField] string _weaponID;
    [SerializeField] List<Transform> _tracer;
    [SerializeField] EquipmentData _weaponDB;

    [Header("Camera Shake")]
    [SerializeField] private ShakeData _combo3Shake;

    [Header("Weapon Sounds")]
    [SerializeField] private string[] _swingSounds;

    public List<Transform> Tracer => _tracer;
    public EquipmentData WeaponDB => _weaponDB;
    public ShakeData Combo3Shake => _combo3Shake;

    [System.Serializable]
    private struct CombatEffectInfo
    {
        public GameObject Prefab;
        public Transform SpawnPoint;
        public bool AttachToParent;

        [Header("Transform Offsets")]
        [Tooltip("체크하면 무기의 회전을 무시하고 플레이어가 바라보는 방향을 기준으로 생성됩니다 (검기 날릴 때 유용)")]
        public bool AlignToPlayerForward;
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;
        [Tooltip("0,0,0이면 프리팹 원본 크기를 사용합니다.")]
        public Vector3 ScaleOffset;

        public float DestroyAfter;


    }

    [Header("Weapon Effects")]
    [SerializeField] private CombatEffectInfo[] _weaponEffects;

    void Awake()
    {
        _weaponDB = DataManager.Instance.GetWeapon(_weaponID);
    }

    public void SpawnEffect(int index)
    {
        if (index < 0 || index >= _weaponEffects.Length || _weaponEffects[index].Prefab == null) return;

        var info = _weaponEffects[index];
        Transform referencePoint = info.SpawnPoint != null ? info.SpawnPoint : transform;

        Vector3 spawnPos = referencePoint.position + referencePoint.TransformDirection(info.PositionOffset);

        Quaternion spawnRot;
        if (info.AlignToPlayerForward)
        {
            // 무기(손목)의 비틀린 회전을 무시하고, 최상위 부모(플레이어)의 회전값을 기준으로 잡음
            Transform playerRoot = transform.root;
            spawnRot = playerRoot.rotation * Quaternion.Euler(info.RotationOffset);
        }
        else
        {
            spawnRot = referencePoint.rotation * Quaternion.Euler(info.RotationOffset);
        }

        GameObject fx = Instantiate(info.Prefab, spawnPos, spawnRot);

        if (info.ScaleOffset != Vector3.zero)
        {
            fx.transform.localScale = info.ScaleOffset;
        }
        if (info.AttachToParent)
        {
            fx.transform.SetParent(referencePoint);

            // 부모 자식 관계가 되었으므로 로컬 좌표계로 깔끔하게 덮어씌움
            fx.transform.localPosition = info.PositionOffset;

            if (info.AlignToPlayerForward)
            {
                // 부모에 종속되면서도 방향은 플레이어 정면을 유지하고 싶을 때
                fx.transform.rotation = transform.root.rotation * Quaternion.Euler(info.RotationOffset);
            }
            else
            {
                fx.transform.localRotation = Quaternion.Euler(info.RotationOffset);
            }
        }

        // 자동 파괴 로직
        if (info.DestroyAfter > 0f)
        {
            Destroy(fx, info.DestroyAfter);
        }

    }

    public string GetSwingSound(int comboStep)
    {
        if (_swingSounds == null || _swingSounds.Length == 0)
            return null;

        int index = Mathf.Clamp(comboStep - 1, 0, _swingSounds.Length - 1);

        return _swingSounds[index];
    }
}