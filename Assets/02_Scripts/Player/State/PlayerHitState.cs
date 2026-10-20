using UnityEngine;

public class PlayerHitState : PlayerState
{
    private float _hitStunDuration = 0.4f;
    private float _timer;

    public PlayerHitState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

   
    public override void Enter()
    {


        _timer = 0f;
        player.Animator.applyRootMotion = false;

        player.Animator.CrossFade("Hit", 0.1f, 0, 0f);

        SoundEvents.RaiseSFXPlayedEvent("PlayerHit");
        player.CurrentMovement = Vector3.zero; 
    }

    public override void LogicUpdate()
    {
        _timer += Time.deltaTime;
        if (_timer >= _hitStunDuration)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void Exit()
    {
        player.Animator.ResetTrigger("Hit");
    }
}