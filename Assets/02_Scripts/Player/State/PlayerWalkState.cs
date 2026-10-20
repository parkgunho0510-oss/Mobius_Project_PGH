using UnityEngine;

public class PlayerWalkState : PlayerState
{
    private float _currentFootSpeedMultiplier = 1f;

    private const float LIFT_THRESHOLD = 0.05f;    // 발이 공중에 있다고 판단할 높이 기준
    private const float AIR_SPEED = 0.7f; // 두 발이 모두 공중에 있을 때의 속도 배율
    private const float LERP_SPEED = 10f;         // 속도가 변하는 부드러움 정도

    //생성자
    public PlayerWalkState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        // 걷기 상태 진입 시 특별히 트리거할 애니메이션이 있다면 호출

        player.CanRotate = true; //회전 가능
        _currentFootSpeedMultiplier = 1f;
    }

    public override void LogicUpdate()
    {


        // 상태 전환 체크 (우선순위가 높은 액션부터 검사)
        if (player.IsAttackTriggered && player.Stats.CurrentStamina > 0)
        {
            stateMachine.ChangeState(player.AttackState);
            return; // 전환 후 아래 이동 로직을 막기 위해 return
        }

        // 입력값이 거의 없으면 Idle 상태로 전환
        if (player.MoveInput.sqrMagnitude <= 0.001f)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        if (player.CharacterController.isGrounded && player.IsRollTriggered && player.Stats.CurrentStamina > 0)
        {
            stateMachine.ChangeState(player.RollState); //구르기
            return;
        }

        if (player.CharacterController.isGrounded && player.IsJumpTriggered)
        {
            stateMachine.ChangeState(player.JumpState); //점프 상태로 전환
            return;
        }

        if (player.IsBlocking && player.Stats.CurrentStamina > 0)
        {
            stateMachine.ChangeState(player.DefenceState);
            return;
        }
        if (player.IsParryTriggered && player.Stats.CurrentStamina >= 30f)
        {
            stateMachine.ChangeState(player.ParryState);
            return;
        }
        // 스프린트 여부에 따라 현재 속도를 배로 적용
        bool canRun = player.IsRun && player.Stats.CurrentStamina > 0;
        float baseSpeed = canRun ? player.MoveSpeed * 2f : player.MoveSpeed;

        if (canRun)
        {
            player.Stats.SpendStamina(3 * Time.deltaTime);
        }

        // 주 로직 실행 (이동 방향만 계산해서 Controller에게 전달)
        if (player.LeftFoot != null && player.RightFoot != null)
        {
            // 캐릭터 루트(중심점)를 기준으로 양발의 상대적 Y축 높이를 구합니다.
            float leftY = player.transform.InverseTransformPoint(player.LeftFoot.position).y;
            float rightY = player.transform.InverseTransformPoint(player.RightFoot.position).y;

            // 두 발 중 '더 낮게 있는 발'의 높이 찾기
            float lowestFootY = Mathf.Min(leftY, rightY);

            // 가장 낮은 발조차 설정한 임계값(LIFT_THRESHOLD)보다 높다면 두 발이 모두 뜬 상태입니다.
            float targetMultiplier = lowestFootY > LIFT_THRESHOLD ? AIR_SPEED : 1.0f;

            // 속도가 순간적으로 변해 캐릭터가 버벅이는 현상을 막기 위해 부드럽게 보간합니다.
            _currentFootSpeedMultiplier = Mathf.Lerp(_currentFootSpeedMultiplier, targetMultiplier, Time.deltaTime * LERP_SPEED);
        }

        float finalCurrentSpeed = baseSpeed * _currentFootSpeedMultiplier;

        // 최종 적용
        Vector3 dir = player.GetCameraDirection(player.MoveInput);

        float lateralMultiplier = 1.0f;

        if (!canRun)
        {
            float absX = Mathf.Abs(player.MoveInput.x);
            float absY = Mathf.Abs(player.MoveInput.y);
            float sum = absX + absY;

            if (sum > 0.001f)
            {
                float lateralRatio = absX / sum;
                lateralMultiplier = Mathf.Lerp(1.0f, 0.7f, lateralRatio);
            }
        }

        // 최종 이동 벡터 산출
        player.CurrentMovement = dir * (finalCurrentSpeed * lateralMultiplier);
    }

    public override void Exit()
    {
        // 걷기 상태 종료 시 특별히 트리거할 애니메이션이 있다면 호출
    }
}