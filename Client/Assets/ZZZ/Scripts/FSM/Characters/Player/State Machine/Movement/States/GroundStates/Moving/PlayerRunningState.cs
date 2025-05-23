using ZZZ;
using UnityEngine.InputSystem;
using System;

namespace TPF
{
    public class PlayerRunningState : PlayerMovementState
    {
        GameTimer gameTimer = null;
        public PlayerRunningState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }
        public override void Enter()
        { 
            base.Enter();
            animator.CrossFadeInFixedTime("WalkStart", 0.14f);

            reusableDate.rotationTime = playerMovementData.runData.rotationTime;

            animator.SetBool(AnimatorID.HasInputID,true);

            reusableDate.inputMult = playerMovementData.runData.inputMult;
        
        }
        public override void Update()
        {
            base.Update();
            
        }
        #region 转换Idling
        protected override void AddInputActionCallBacks()
        {
            base.AddInputActionCallBacks();
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.canceled += OnEnterIdle;
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.started += OnKeepRunning;
        }

        protected override void RemoveInputActionCallBacks()
        {
            base.RemoveInputActionCallBacks();
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.canceled -= OnEnterIdle;
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.started -= OnKeepRunning;
        }

        private void OnEnterIdle(InputAction.CallbackContext context)
        {
            gameTimer = ZZZTimerManager.MainInstance.GetTimer(playerMovementData.bufferToIdleTime, StartToIdle);
        }

        private void StartToIdle()
        {
            movementStateMachine.ChangeState(movementStateMachine.idlingState);
        }
        private void OnKeepRunning(InputAction.CallbackContext context)
        {
            //注销原来的计时器
            ZZZTimerManager.MainInstance.UnregisterTimer(gameTimer);
            //播发动画
            animator.CrossFadeInFixedTime("WalkStart", 0.14f);
        }


        #endregion

        #region 转换Walking

        protected override void OnWalkStart(InputAction.CallbackContext context)
        {
            base.OnWalkStart(context);

            movementStateMachine.ChangeState(movementStateMachine.walkingState);
        }
        #endregion
    }
}

