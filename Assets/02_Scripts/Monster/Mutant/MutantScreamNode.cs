using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class MutantScreamNode : BTNode
{
    Monster_Mutant_Scout _mutant;

    bool _isUsed = false;
    bool _screamingDone = false;
    float _screamTimer = 0f;

    public MutantScreamNode(Monster_Mutant_Scout mutant)
    {
        _mutant = mutant;
    }

    public override NodeState Evaluate()
    {
        if (_mutant.IsDead)
            return State = NodeState.Failure;

        if (_isUsed)
            return State = NodeState.Failure;

        if (_mutant.DistanceToPlayer() > _mutant.DetectRange)
            return State = NodeState.Failure;

        if (!_screamingDone)
        {
            _screamingDone = true;
            _screamTimer = 0f;

            _mutant.Agent.velocity = Vector3.zero;
            _mutant.Agent.isStopped = true;
            _mutant.transform.LookAt(_mutant.Player.transform);

            

            SoundEvents.RaiseSFXPlayedEvent("Mutant_Scream");
            _mutant.Animator.SetTrigger("Scream");
        }

        _screamTimer += Time.deltaTime;

        if (_screamTimer > 2.07f)
        {
            if (_mutant.IsDead)
                return State = NodeState.Failure;

            _isUsed = true;

            _mutant.Animator.SetBool("IsMove", true);
            _mutant.Agent.speed = _mutant.MoveSpeed * 1.5f;

            _mutant.Agent.ResetPath();
            _mutant.Agent.velocity = Vector3.zero;
            _mutant.Agent.isStopped = false;

            return State = NodeState.Success;
        }

        return State = NodeState.Running;
    }

    public override void Abort()
    {
        base.Abort();

        if (_mutant == null)
            return;

        if (_mutant.IsDead)
        {
            _mutant.Animator.ResetTrigger("Scream");
            _mutant.Animator.SetBool("IsMove", false);

            if (_mutant.Agent != null && _mutant.Agent.enabled)
            {
                _mutant.Agent.isStopped = true;
                _mutant.Agent.velocity = Vector3.zero;
            }
        }
    }
}