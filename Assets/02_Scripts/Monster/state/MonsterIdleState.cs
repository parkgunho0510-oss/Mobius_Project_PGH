using System.Collections;
using System.Threading;
using UnityEngine;

public class MonsterIdleState : MonsterState
{
    //private float _idleTime = 1.5f;
    private float _idleTime;
    private float _timer;
    //private Coroutine _coroutine;

    public MonsterIdleState(MonsterBase monster, MonsterStateMachine stateMachine)
        : base(monster, stateMachine) { }

    public override void Enter()
    {
        _monster.Agent.isStopped = false;

        _monster.Animator.SetBool("IsMove", false);

        _idleTime = Random.Range(1.2f, 2.0f);
        _timer = 0f;
        //_coroutine = _monster.StartCoroutine(Co_Idle());
    }

    public override void LogicUpdate()
    {
        _timer += Time.deltaTime;
        if (_timer >= _idleTime)
        {
            _stateMachine.ChangeState(_monster.PatrolState);
        }
    }
    /*private IEnumerator Co_Idle()
    {
        yield return new WaitForSeconds(_idleTime);
        _stateMachine.ChangeState(_monster.PatrolState);
    }
    */
    public override void Exit()
    {
        //if (_coroutine != null) _monster.StopCoroutine(_coroutine);
    }
}