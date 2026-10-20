using System.Collections;
using UnityEngine;

public class KnightChargeAttackNode : BTNode
{
    private Monster_Knight_Scout _knight;
    private Coroutine _coroutine;
    private bool _isAttacking = false;
    private bool _isAnimationFinished = false;

    // 쿨타임 설정
    private float _cooldown = 15f;
    private float _lastChargeTime = -999f;

    public KnightChargeAttackNode(Monster_Knight_Scout knight)
    {
        _knight = knight;
    }

    public override NodeState Evaluate()
    {
        if (!_isAttacking)
        {
            // 거리 및 쿨타임 검사
            if (_knight.DistanceToPlayer() > _knight.DetectRange || Time.time - _lastChargeTime < _cooldown)
            {
                return State = NodeState.Failure;
            }
        }

        // 공격 시작
        if (!_isAttacking)
        {
            _isAttacking = true;
            _isAnimationFinished = false;

            if (_knight.Combat != null) _knight.Combat.OnAttackFinished += OnAnimationComplete;
            _coroutine = _knight.StartCoroutine(Co_ChargeAttack());

            // 차지 발동 시간 기록
            _lastChargeTime = Time.time;
        }

        return State = NodeState.Running;
    }

    private void OnAnimationComplete()
    {
        _isAnimationFinished = true;
    }

    private IEnumerator Co_ChargeAttack()
    {
        _knight.Agent.isStopped = true;
        _knight.Agent.velocity = Vector3.zero;
        _knight.Animator.SetBool("IsMove", false);

        _knight.RotateToTarget(10);
        yield return new WaitForSeconds(0.3f);

        Vector3 dashDirection = (_knight.Player.transform.position - _knight.transform.position).normalized;
        dashDirection.y = 0;

        _knight.Animator.SetBool("IsCharge", true);
        if (_knight.Combat != null) _knight.Combat.SetActiveHitboxGroup("BodyBox");

        float timer = 0f;

        float bumpDistance = 1f;
        while (timer < _knight.ChargeDuration && !_isAnimationFinished)
        {
            timer += Time.deltaTime;

            if (_knight.DistanceToPlayer() <= bumpDistance) break;

            _knight.Agent.Move(dashDirection * _knight.ChargeSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        FinishAttack();
    }

    private void FinishAttack()
    {
        // 돌진 로직이 완전히 끝나면 Bool 끄기
        _knight.Animator.SetBool("IsCharge", false);

        if (_knight.Combat != null)
        {
            _knight.Combat.ForceStopAttack();
            _knight.Combat.OnAttackFinished -= OnAnimationComplete;
        }

        _isAttacking = false;
        _coroutine = null;
        _knight.Agent.isStopped = false;

        State = NodeState.Success;
    }

    public override void Abort()
    {
        base.Abort();
        if (_isAttacking)
        {
            // 스턴이나 사망으로 강제 중단될 때도 확실하게 Bool 끄기
            _knight.Animator.SetBool("IsCharge", false);

            if (_coroutine != null) _knight.StopCoroutine(_coroutine);
            if (_knight.Combat != null)
            {
                _knight.Combat.ForceStopAttack();
                _knight.Combat.OnAttackFinished -= OnAnimationComplete;
            }
            _isAttacking = false;
            _isAnimationFinished = false;
        }
    }
}