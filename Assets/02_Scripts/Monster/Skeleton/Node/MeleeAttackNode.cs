using System.Collections;
using UnityEngine;

public class MeleeAttackNode : MonsterAttackNode
{
    private string _hitboxGroupName;

    // MonsterBase로 업캐스팅하여 받고, 히트박스 이름과 후딜레이를 매개변수화
    public MeleeAttackNode(MonsterBase monster, string hitboxGroupName = "MeleeCollider", float delay = 1.5f) : base(monster)
    {
        _hitboxGroupName = hitboxGroupName;
        _delay = delay;
    }

    protected override IEnumerator Co_ExecuteAttack()
    {
        _monster.RotateToTarget(5);
        yield return new WaitForSeconds(0.2f);

        _monster.Animator.SetTrigger("Attack");

        if (_combat != null)
        {
            _combat.SetActiveHitboxGroup(_hitboxGroupName);
        }
    }

    public override void Abort()
    {
        base.Abort();

        if (_monster != null && _monster.Animator != null)
        {
            _monster.Animator.ResetTrigger("Attack");
        }

    }
}