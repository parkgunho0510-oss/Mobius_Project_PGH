using UnityEngine;

public class BossChaseNode : BTNode
{
    private BossBase _boss;
    private bool _hasRangedAttack;

    private float _pathUpdateDelay = 0.2f;
    private float _lastTimePathUpdated = 0f;
    public BossChaseNode(BossBase boss, bool hasRangedAttack = true)
    {
        _boss = boss;
        _hasRangedAttack = hasRangedAttack;
    }

    public override NodeState Evaluate()
    {
        // 예외: 플레이어가 죽었으면 추적 실패 반환
        if (_boss.IsPlayerDead()) return State = NodeState.Failure;

        float dist = _boss.DistanceToPlayer();

        // 목표 사거리 안에 들어오면 추적 성공(Success) 반환 -> 공격 노드로 넘어감
        if (dist <= _boss.AttackRange)
        {
            _boss.Agent.isStopped = true;
            return State = NodeState.Success;
        }

        if (_hasRangedAttack && dist >= _boss.AttackRange * 3f)
        {
            _boss.Agent.isStopped = true;
            return State = NodeState.Success;
        }

        // 아직 멀었으면 계속 추적(Running)
        _boss.Agent.isStopped = false;
        if (Time.time - _lastTimePathUpdated > _pathUpdateDelay)
        {
            _boss.Agent.SetDestination(_boss.Player.transform.position);
            _lastTimePathUpdated = Time.time;
        }
        _boss.RotateToTarget();

        return State = NodeState.Running;
    }

    public override void Abort()
    {
        base.Abort();
        _boss.Agent.isStopped = true;
    }
}