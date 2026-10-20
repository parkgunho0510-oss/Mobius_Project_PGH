using UnityEngine;

public class PlayerWeaponSwitchState : PlayerState
{
    private bool _hasSwapped;

    public PlayerWeaponSwitchState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _hasSwapped = false; // 진입 시 초기화

        player.CurrentMovement = Vector3.zero;
        player.CanRotate = false;

        player.Animator.Play("wp_unequip", 0, 0f);
    }

    // 애니메이션의 wp_unequip 중간(무기를 등에 넣는 시점)에 호출될 메서드
    public void PerformSwap()
    {
        if (_hasSwapped) return; 

        _hasSwapped = true;
        player.GetComponent<WeaponSwitcher>().SwitchWeapon();
    }

    public override void Exit()
    {
        player.CanRotate = true;
    }
}