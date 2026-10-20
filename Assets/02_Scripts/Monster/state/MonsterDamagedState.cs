using System.Collections;
using System.Threading;
using UnityEngine;

public class MonsterDamagedState : MonsterState
{
    private Coroutine _coroutine;

    public MonsterDamagedState(MonsterBase monster, MonsterStateMachine stateMachine)
        : base(monster, stateMachine) { }

    public override void Enter()
    {
        _monster.Agent.isStopped = true;
        _monster.Agent.velocity = UnityEngine.Vector3.zero;

        _monster.Animator.SetTrigger("Hit");
        _coroutine = _monster.StartCoroutine(Co_Damaged());
    }

    private IEnumerator Co_Damaged()
    {
        float knockbackDuration = 0.2f; // 밀려나는 시간
        float knockbackSpeed = 5.0f;    // 밀려나는 속도 (넉백 파워)
        float timer = 0f;

        while (timer < knockbackDuration)
        {
            // Time.deltaTime을 곱해주어 프레임 보정
            Vector3 moveOffset = _monster.LastKnockbackDir * (knockbackSpeed * Time.deltaTime);
            _monster.Agent.Move(moveOffset);

            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(Mathf.Max(0, 0.5f - knockbackDuration));

        if (_monster.CurrentHp <= 20 && _monster.EvolutionData != null)
        {
            _stateMachine.ChangeState(_monster.EvolutionState);
        }
        else
        {
            _stateMachine.ChangeState(_monster.CombatState);
        }
    }

    public override void Exit()
    {
        if (_coroutine != null) _monster.StopCoroutine(_coroutine);
        _monster.Agent.isStopped = false;
    }
}