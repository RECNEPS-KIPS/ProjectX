using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ZZZ
{
    public class PlayerNullState : PlayerComboState
    {
        //如果父类实现了带参的构造函数，那么子类也要实现一个赋值调用给父类的构造函数
        public PlayerNullState(PlayerComboStateMachine comboStateMachine ) : base(comboStateMachine)
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
