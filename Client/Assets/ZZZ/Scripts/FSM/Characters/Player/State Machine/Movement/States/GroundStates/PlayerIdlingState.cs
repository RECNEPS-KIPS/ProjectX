
using ZZZ;
using UnityEngine;
using UnityEngine.InputSystem;
using GGG.Tool;

namespace TPF
{
    public class PlayerIdlingState : PlayerMovementState
    {
        GameTimer GameTimer { get; set; }
        //调用父类的构造函数
        public PlayerIdlingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {

        }
        public override void Enter()
        {  
            base.Enter();
            reusableDate.rotationTime= playerMovementData.idleData.rotationTime;
            animator.SetBool(AnimatorID.HasInputID, false);
            reusableDate.inputMult = playerMovementData.idleData.inputMult;

        }
        public override void Update()
        {
            base.Update();
            ////没有输入就退出
            //if (CharacterInputSystem.MainInstance.PlayerMove==Vector2.zero)
            //{
            //    return;
            //}
            //else //有输入执行跳转
            //{
               
            //}
        }
        protected override void AddInputActionCallBacks()
        {
            base.AddInputActionCallBacks();
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.started += bufferToRun;
        }
        protected override void RemoveInputActionCallBacks() 
        {
            base.RemoveInputActionCallBacks();
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.started -= bufferToRun;
            ZZZTimerManager.MainInstance.UnregisterTimer(GameTimer);
        }

        private void bufferToRun(InputAction.CallbackContext context)
        {          
          GameTimer=ZZZTimerManager.MainInstance.GetTimer(0.11f,CheckMoveInput);
        }

        private void CheckMoveInput()
        {
           
            //视为轻击角色没有Walk或者Run而是Run_Start_End
            if (CharacterInputSystem.MainInstance.PlayerMove == Vector2.zero)
            {
                 animator.CrossFadeInFixedTime("Run_Start_End",0.13f);
            }
            else
            {
                Move();
            }
        }

        private void Move()
        {
            if (movementStateMachine.reusableDate.shouldWalk)
            {
                //切换到Walk状态
                movementStateMachine.ChangeState(movementStateMachine.walkingState);
                return;
            }
            //否则执行Run移动
            movementStateMachine.ChangeState(movementStateMachine.runningState);
        }

        public override void HandInput()
        {
            base.HandInput();

        }
        public override void Exit()
        {
            base.Exit();
        }
    }
}
