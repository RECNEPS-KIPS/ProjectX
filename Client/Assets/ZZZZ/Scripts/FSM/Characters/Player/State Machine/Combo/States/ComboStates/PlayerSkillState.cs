using ZZZ;
using UnityEngine;

public class PlayerSkillState : PlayerComboState
{
    public PlayerSkillState(PlayerComboStateMachine comboStateMachine) : base(comboStateMachine)
    {
    }

    /// <summary>
    /// </summary>
    public override void Enter()
    {
        base.Enter();
        comboStateMachine.Player.movementStateMachine.ChangeState(comboStateMachine.Player.movementStateMachine
            .playerMovementNullState);
        CameraSwitcher.MainInstance.ActiveStateCamera(player.characterName, reusableData.currentSkill.attackStyle);
    }

    public override void Update()
    {
        characterCombo.UpdateAttackLookAtEnemy();
    }

    public override void Exit()
    {
        CameraSwitcher.MainInstance.UnActiveStateCamera(player.characterName, reusableData.currentSkill.attackStyle);
        base.Exit();
    }

    /// <summary>
    /// </summary>
    public override void OnAnimationExitEvent()
    {
        comboStateMachine.ChangeState(comboStateMachine.NullState);
    }

    /// <summary>
    /// </summary>
    /// <param name="state"></param>
    public override void OnAnimationTranslateEvent(IState state)
    {
        comboStateMachine.ChangeState(state);
    }
}