using System.Collections.Generic;
using UnityEngine;

public class Monster_Knight_Scout : MonsterBase
{
    [Header("Equipment")]
    [SerializeField] private MonsterCombat _combat;

    [Header("Charge Attack Settings")]
    public float ChargeSpeed = 10f;
    public float ChargeDuration = 1.0f;

    public MonsterCombat Combat => _combat;

    public override BTNode GetAttackNode()
    {
        return new Selector(new List<BTNode>
        {
         new Sequence(new List<BTNode>
            {
                new KnightChargeAttackNode(this),
                new MeleeAttackNode(this, "MeleeCollider", 0f) 
            }),

            new MeleeAttackNode(this, "MeleeCollider", 1.5f)
        });
    }
}