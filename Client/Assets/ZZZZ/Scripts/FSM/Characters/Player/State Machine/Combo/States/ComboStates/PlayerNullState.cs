using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZZZ
{
    public class PlayerNullState : PlayerComboState
    {
        public PlayerNullState(PlayerComboStateMachine comboStateMachine) : base(comboStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            characterCombo.ReSetComboInfo();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnAnimationTranslateEvent(IState state)
        {
            comboStateMachine.ChangeState(state);
        }
    }
}