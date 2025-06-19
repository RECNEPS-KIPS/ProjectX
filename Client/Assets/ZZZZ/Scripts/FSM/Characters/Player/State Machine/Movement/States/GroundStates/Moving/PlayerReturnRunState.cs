using UnityEngine;
using GGG.Tool;

namespace ZZZ
{
    public class PlayerReturnRunState : PlayerMovementState
    {
        public PlayerReturnRunState(PlayerMovementStateMachine playerMovementStateMachine) : base(
            playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            animator.SetBool(AnimatorID.TurnBackID, false);

            animator.SetBool(AnimatorID.HasInputID, true);

            reusableDate.inputMult = playerMovementData.returnRunData.inputMult;

            reusableDate.rotationTime = playerMovementData.returnRunData.rotationTime;
        }

        public override void Update()
        {
            if (animator.AnimationAtTag("TurnRun") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.08f)
            {
                return;
            }

            CharacterRotation(GetPlayerMovementInputDirection());
        }

        public override void Exit()
        {
            base.Exit();

            animator.SetBool(AnimatorID.TurnBackID, false);
        }

        /// <summary>
        /// </summary>
        public override void OnAnimationExitEvent()
        {
            if (CharacterInputSystem.MainInstance.PlayerMove == Vector2.zero)
            {
                movementStateMachine.ChangeState(movementStateMachine.idlingState);
                return;
            }

            movementStateMachine.ChangeState(movementStateMachine.sprintingState);
        }
    }
}