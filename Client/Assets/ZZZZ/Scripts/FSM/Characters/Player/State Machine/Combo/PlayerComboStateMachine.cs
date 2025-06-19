using UnityEngine;


namespace ZZZ
{
    public class PlayerComboStateMachine : StateMachine
    {
        public Player Player { get; }
        public PlayerATKIngState ATKIngState { get; }
        public PlayerNullState NullState { get; }
        public PlayerComboReusableData ReusableData { get; }

        public PlayerSkillState SkillState { get; }

        public PlayerComboStateMachine(Player player)
        {
            Player = player;

            ReusableData = new PlayerComboReusableData();

            ATKIngState = new PlayerATKIngState(this);

            NullState = new PlayerNullState(this);

            SkillState = new PlayerSkillState(this);
        }
    }
}