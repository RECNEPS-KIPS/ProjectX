using GGG.Tool;

namespace ZZZ
{
    public class PlayerATKIngState : PlayerComboState
    {
        public PlayerATKIngState(PlayerComboStateMachine comboStateMachine) : base(comboStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();

            characterCombo.UpdateAttackLookAtEnemy();

            characterCombo.CheckMoveInterrupt();
        }

        #region

        public void CancelAttackColdTime()
        {
            characterCombo.CanATK();
        }

        public void EnablePreInput()
        {
            characterCombo.CanInput();
        }

        public void EnableMoveInterrupt()
        {
            characterCombo.CanMoveInterrupt();
        }

        public void DisableLinkCombo()
        {
            characterCombo.DisConnectCombo();
        }

        /// <summary>
        /// </summary>
        public void ATK()
        {
            characterCombo.ATK();
        }

        #endregion

        /// <summary>
        /// </summary>
        public override void OnAnimationExitEvent()
        {
            ZZZZTimerManager.MainInstance.GetOneTimer(0.2f, ToNullState);
        }

        private void ToNullState()
        {
            if (!animator.AnimationAtTag("ATK"))
            {
                comboStateMachine.ChangeState(comboStateMachine.NullState);
                return;
            }
        }

        public override void OnAnimationTranslateEvent(IState state)
        {
            comboStateMachine.ChangeState(state);
        }
    }
}