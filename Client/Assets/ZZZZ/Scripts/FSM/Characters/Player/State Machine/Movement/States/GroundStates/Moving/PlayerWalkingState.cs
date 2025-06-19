using UnityEngine;
using UnityEngine.InputSystem;

namespace ZZZ
{
    public class PlayerWalkingState : PlayerMovementState
    {
        public PlayerWalkingState(PlayerMovementStateMachine playerMovementStateMachine) : base(
            playerMovementStateMachine)
        {
        }

        GameTimer gameTimer;

        public override void Enter()
        {
            base.Enter();
            reusableDate.rotationTime = playerMovementData.walkData.rotationTime;

            animator.SetBool(AnimatorID.HasInputID, true);

            reusableDate.inputMult = playerMovementData.walkData.inputMult;
        }

        public override void Update()
        {
            base.Update();
        }

        #region

        protected override void AddInputActionCallBacks()
        {
            base.AddInputActionCallBacks();

            CharacterInputSystem.MainInstance.inputActions.Player.Movement.canceled += OnBufferToIdle;
        }

        protected override void RemoveInputActionCallBacks()
        {
            base.RemoveInputActionCallBacks();
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.canceled -= OnBufferToIdle;
        }

        private void OnBufferToIdle(InputAction.CallbackContext context)
        {
            gameTimer = ZZZZTimerManager.MainInstance.GetTimer(playerMovementData.bufferToIdleTime, IdleStart);
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.started += OnUnregisterBufferTimer;
        }


        private void IdleStart()
        {
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.started -= OnUnregisterBufferTimer;
            movementStateMachine.ChangeState(movementStateMachine.idlingState);
            //movementStateMachine.ChangeState(movementStateMachine.idlingState);
        }

        private void OnUnregisterBufferTimer(InputAction.CallbackContext context)
        {
            ZZZZTimerManager.MainInstance.UnregisterTimer(gameTimer);
        }

        #endregion

        #region

        protected override void OnWalkStart(InputAction.CallbackContext context)
        {
            base.OnWalkStart(context);

            movementStateMachine.ChangeState(movementStateMachine.runningState);
        }

        #endregion
    }
}