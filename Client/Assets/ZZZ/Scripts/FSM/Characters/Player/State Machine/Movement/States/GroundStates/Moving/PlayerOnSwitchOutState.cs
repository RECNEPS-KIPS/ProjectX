using UnityEngine;
namespace ZZZ
{
    public class PlayerOnSwitchOutState : PlayerMovementState
    {
        public PlayerOnSwitchOutState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            //获取当前对象类型的名称
            Debug.Log(movementStateMachine.player.characterName + "的状态为" + GetType().Name);
        }
        public override void Update()
        {
          //角色不能旋转
        }

        //这个状态的唯一出路时SwitchIn，进入和退出都是通过写入Animation的函数触发
    }
}
