
using ZZZ;
using UnityEngine;

public class PlayerDashingState : PlayerMovementState
{
    PlayerDashData dashData;

    public PlayerDashingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
    {
        dashData = playerMovementData.dashData;
    }
    //ʵ���ڲ��߼�
    public override void Enter()
    {
        base.Enter();
        reusableDate.rotationTime = playerMovementData.dashData.rotationTime;
        
        reusableDate.canDash = false;
        ZZZZTimerManager.MainInstance.GetOneTimer(playerMovementData.dashData.coldTime, ResetDash);
        //������Ч
        movementStateMachine.player.PlayDodgeSound();
    }
   
    public override void Update() 
    {
        base.Update();
    }
   
   
    #region Dashת�� Idle?Sprint
    public override void OnAnimationExitEvent()
    {
        if (CharacterInputSystem.MainInstance.PlayerMove == Vector2.zero)
        {
            movementStateMachine.ChangeState(movementStateMachine.idlingState);
            return;
        }
        movementStateMachine.ChangeState(movementStateMachine.sprintingState);
    }
    #endregion

}
