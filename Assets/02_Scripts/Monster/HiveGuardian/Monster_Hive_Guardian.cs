using System.Collections.Generic;
using UnityEngine;

public class Monster_Hive_Guardian : MonsterBase
{
    [Header("Ranged Equipment")]
    [SerializeField] private FireBreath _fireBreathPrefab;
    [SerializeField] private Transform _firePoint;

    protected override void Update()
    {
        base.Update();

        if (!IsDead && Player != null && FindTarget())
        {
            RotateToTarget();
        }
    }

    public override BTNode GetAttackNode()
    {
        return new Selector(new List<BTNode>
        {  
            // 체력이 낮으면 도망
            new FleeNode(this, safeDistance: 25f),

            // 거리가 확보되었다면 원거리 공격 실행
            new RangedAttackNode(this)
        });
    }

    // 이 메서드는 애니메이션 클립 Animation Event로 넣고 호출
    public void ShootFireBreath()
    {
        if (_fireBreathPrefab == null || _firePoint == null || Player == null)
            return;

        Vector3 spawnPos =
            _firePoint.position + _firePoint.forward * 2.0f;

        FireBreath breath =
            Instantiate(_fireBreathPrefab,
            spawnPos,
            Quaternion.identity);

        Vector3 targetPos = Player.transform.position;
        Vector3 direction = (targetPos - _firePoint.position).normalized;

        breath.Breath(direction, Damage);

        // 사운드가 필요하다면 추가
        // SoundEvents.RaiseSFXPlayedEvent(
    }
}
