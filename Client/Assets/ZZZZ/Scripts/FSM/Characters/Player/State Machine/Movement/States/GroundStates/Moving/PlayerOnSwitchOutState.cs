using UnityEngine;

namespace ZZZ
{
    public class PlayerOnSwitchOutState : PlayerMovementState
    {
        public PlayerOnSwitchOutState(PlayerMovementStateMachine playerMovementStateMachine) : base(
            playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            Debug.Log(movementStateMachine.player.characterName + " GetTypeName:" + GetType().Name);
        }

        public override void Update()
        {
        }
    }
}