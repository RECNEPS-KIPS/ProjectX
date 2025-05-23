using GGG.Tool;

namespace ZZZ
{
    public class PlayerATKIngState : PlayerComboState
    {
        public PlayerATKIngState(PlayerComboStateMachine comboStateMachine) : base(comboStateMachine) { }
        public override void Enter()
        {
            base.Enter();
           
        }
        public override void Update()
        {
            base .Update();
        
            characterCombo.UpdateAttackLookAtEnemy();

            characterCombo.CheckMoveInterrupt();
        }
        #region 攻击事件
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
        /// ATK这是攻击触发的核心事件，包括了伤害、受击动画、格挡攻击、攻击者、打击感（震屏、顿帧）、受击音效、受击特效
        /// </summary>
        public void ATK()
        { 
           characterCombo.ATK();
        }
        #endregion
        /// <summary>
        /// 动画事件退出:等攻击动画播放完退出
        /// </summary>
        public override void OnAnimationExitEvent()
        {
            ZZZTimerManager.MainInstance.GetOneTimer(0.2f, ToNullState);
        }

        private void ToNullState()
        {
            if (!animator.AnimationAtTag("ATK"))
            {
                comboStateMachine.ChangeState(comboStateMachine.NullState);
                return;
            }
        }
        //闪避退出
        public override void OnAnimationTranslateEvent(IState state)
        {
            comboStateMachine.ChangeState(state);
        }

    }
}
