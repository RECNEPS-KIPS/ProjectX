
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
    //实现内部逻辑
    public override void Enter()
    {
        base.Enter();
        reusableDate.rotationTime = playerMovementData.dashData.rotationTime;

        reusableDate.canDash = false;
        ZZZTimerManager.MainInstance.GetOneTimer(playerMovementData.dashData.coldTime, ResetDash);
        //播放音效
        movementStateMachine.player.PlayDodgeSound();
    }
    public override void Update()
    {
        if (dashData.dodgeBackApplyRotation)
        {
            base.Update();
        }
    }
    #region Dash转到 Idle?Run
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
