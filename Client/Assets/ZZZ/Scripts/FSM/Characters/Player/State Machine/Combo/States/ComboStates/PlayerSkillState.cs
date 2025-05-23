
using ZZZ;
using UnityEngine;

public class PlayerSkillState : PlayerComboState
{
    public PlayerSkillState(PlayerComboStateMachine comboStateMachine) : base(comboStateMachine)
    {
    }
    /// <summary>
    /// 通过输入事件和执行条件触发
    /// </summary>
    public override void Enter()
    {
        base.Enter();
        comboStateMachine.Player.movementStateMachine.ChangeState(comboStateMachine.Player.movementStateMachine.playerMovementNullState);
        //激活状态相机-这里修改一下传入StateDriveCameras，从连招里面获取
        CameraSwitcher.MainInstance.ActiveStateCamera(player.characterName, reusableData.currentSkill.attackStyle);
      
    }

    public override void Update()
    {
        characterCombo.UpdateAttackLookAtEnemy();
    }
    public override void Exit()
    {
        CameraSwitcher.MainInstance.UnActiveStateCamera(player.characterName,reusableData.currentSkill.attackStyle);
        base.Exit();
      
    }
    /// <summary>
    /// 通过动画脚本触发：动画播放完退出
    /// </summary>
    public override void OnAnimationExitEvent()
    {
        comboStateMachine.ChangeState(comboStateMachine.NullState);
       
    }
    /// <summary>
    /// 进入switchout动画退出
    /// </summary>
    /// <param name="state"></param>
    public override void OnAnimationTranslateEvent(IState state)
    {
        comboStateMachine.ChangeState(state);
    }
}
