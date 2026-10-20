using System.Collections;
using UnityEngine;

public class RangedAttackNode : MonsterAttackNode
{
    //private Monster_Skeleton_Archer _archer;

    public RangedAttackNode(MonsterBase monster) : base(monster)
    {
        _monster = monster;
        _delay = 2.0f; //  시간 설정
    }

    protected override IEnumerator Co_ExecuteAttack()
    {
        _monster.RotateToTarget();
        _monster.Animator.SetTrigger("Attack");

        yield break;
    }

    public override void Abort()
    {
        base.Abort();
        var combat = _monster.GetComponent<MonsterCombat>();
        if (combat != null) combat.StopCasting();
    }
}