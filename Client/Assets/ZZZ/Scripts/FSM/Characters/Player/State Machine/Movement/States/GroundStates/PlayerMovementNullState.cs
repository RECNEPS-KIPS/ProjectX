
using UnityEngine;
using GGG.Tool;

namespace ZZZ
{
    public class PlayerMovementNullState : PlayerMovementState
    {
        public PlayerMovementNullState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }
        public override void Enter()
        {
          //base 包含了过渡到闪避的委托，如果需要控制攻击时的闪避时间，则删掉这里然后重写
             base.Enter();
            reusableDate.rotationTime = playerMovementData.comboRotaionTime;

        }
        
        public override void Update()
        {
            //实现在攻击时的转向
            if (animator.AnimationAtTag("ATK"))
            {
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < playerMovementData.comboRotationPercentage)
                {
                    base.Update();
                }   
            }
        }
     
        public override void Exit()
        {
            base.Exit();
            
        }
        //ATK动画或者技能动画播放完时出发
        public override void OnAnimationExitEvent()
        {
            ZZZTimerManager.MainInstance.GetOneTimer(0.2f, CheckStateExit);
        }

        private void CheckStateExit()
        {
            if (animator.AnimationAtTag("ATK") || animator.AnimationAtTag("Skill"))
            {
                return;
            }
            if (CharacterInputSystem.MainInstance.PlayerMove != Vector2.zero)
            {
                movementStateMachine.ChangeState(movementStateMachine.runningState);
                return;
            }
            movementStateMachine.ChangeState(movementStateMachine.idlingState);
        }
    }
}