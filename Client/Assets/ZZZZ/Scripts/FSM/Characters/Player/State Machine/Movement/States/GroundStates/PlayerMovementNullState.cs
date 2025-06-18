
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
          //base �����˹��ɵ����ܵ�ί�У������Ҫ���ƹ���ʱ������ʱ�䣬��ɾ������Ȼ����д
             base.Enter();
            reusableDate.rotationTime = playerMovementData.comboRotaionTime;

        }
        
        public override void Update()
        {
            //ʵ���ڹ���ʱ��ת��
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
        //ATK�������߼��ܶ���������ʱ����
        public override void OnAnimationExitEvent()
        {
            ZZZZTimerManager.MainInstance.GetOneTimer(0.2f, CheckStateExit);
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