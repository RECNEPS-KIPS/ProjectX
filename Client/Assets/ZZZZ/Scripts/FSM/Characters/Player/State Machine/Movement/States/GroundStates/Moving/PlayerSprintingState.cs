using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GGG.Tool;
using UnityEngine.InputSystem;

namespace ZZZ
{
    public class PlayerSprintingState : PlayerMovementState
    {
        GameTimer gameTimer;
        Vector3 targetDir;
        float turnDeltaAngle;
        public PlayerSprintingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }
        public override void Enter()
        {
            base.Enter();
            reusableDate.rotationTime = playerMovementData.sprintData.rotationTime;

            animator.SetBool(AnimatorID.HasInputID, true);

            reusableDate.inputMult = playerMovementData.sprintData.inputMult;


        }
        public override void Update()
        {
            base.Update();
            targetDir = Quaternion.Euler(0, reusableDate.targetAngle, 0) * Vector3.forward;
            turnDeltaAngle = DevelopmentToos.GetDeltaAngle(playerTransform, targetDir);

            if (Mathf.Abs(turnDeltaAngle) > playerMovementData.turnBackAngle)
            {
                animator.SetBool(AnimatorID.TurnBackID, true);
              
            }
           
           
        }
        public override void Exit()
        {
            base.Exit();
           
        }
        #region ת��Idling
        protected override void AddInputActionCallBacks()
        {
            base.AddInputActionCallBacks();

            CharacterInputSystem.MainInstance.inputActions.Player.Movement.canceled += OnBufferToIdle;

        }
        protected override void RemoveInputActionCallBacks()
        {
            base.RemoveInputActionCallBacks();
            Debug.Log("�Ƴ�run��idle��ί��");
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.canceled -= OnBufferToIdle;
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.started -= OnUnregisterBufferTimer;

        }

        private void OnBufferToIdle(InputAction.CallbackContext context)
        {
            gameTimer = ZZZZTimerManager.MainInstance.GetTimer(playerMovementData.bufferToIdleTime, IdleStart);
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.started += OnUnregisterBufferTimer;
        }



        private void IdleStart()
        {           
            movementStateMachine.ChangeState(movementStateMachine.idlingState);          
        }
        private void OnUnregisterBufferTimer(InputAction.CallbackContext context)
        {
            Debug.Log("ע��Timer");
            ZZZZTimerManager.MainInstance.UnregisterTimer(gameTimer);
        }
        #endregion

        #region ת����walk
        protected override void OnWalkStart(InputAction.CallbackContext context)
        {
            base.OnWalkStart(context);
            movementStateMachine.ChangeState(movementStateMachine.walkingState);
        }
        #endregion
    }
}
