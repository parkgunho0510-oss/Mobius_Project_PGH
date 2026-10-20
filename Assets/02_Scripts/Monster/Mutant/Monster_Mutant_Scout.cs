using System.Collections.Generic;

public class Monster_Mutant_Scout : MonsterBase
{
    public override BTNode GetAttackNode()
    {
        return new Selector(new List<BTNode>
        {
            new MutantScreamNode(this),
            new MeleeAttackNode(this)
        });
    }
}
