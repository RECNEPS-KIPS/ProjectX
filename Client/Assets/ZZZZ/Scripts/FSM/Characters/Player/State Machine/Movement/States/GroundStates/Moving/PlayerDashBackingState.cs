
using ZZZ;
using UnityEngine;
using static UnityEngine.Mesh;

public class PlayerDashBackingState : PlayerMovementState
{
    PlayerDashData dashData;
    public PlayerDashBackingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
    {
        dashData = playerMovementData.dashData;
    }
    public override void Enter()
    {
        base.Enter();
        reusableDate.rotationTime = playerMovementData.dashData.rotationTime;

        reusableDate.canDash = false;
        ZZZZTimerManager.MainInstance.GetOneTimer(playerMovementData.dashData.coldTime, ResetDash);
        movementStateMachine.player.PlayDodgeSound();
    }
    public override void Update()
    {
        if (dashData.dodgeBackApplyRotation)
        {
            base.Update();
        }
    }
    #region
    public override void OnAnimationExitEvent()
    {
        if (CharacterInputSystem.MainInstance.PlayerMove == Vector2.zero)
        {
            movementStateMachine.ChangeState(movementStateMachine.idlingState);
            return;
        }
        movementStateMachine.ChangeState(movementStateMachine.runningState);
    }
    #endregion

}
