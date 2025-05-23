
using TPF;
using Unity.VisualScripting;

namespace ZZZ
{
    public class PlayerMovementStateMachine : StateMachine
    {
        //缓存初始状态
        public PlayerStateReusableDate reusableDate { get;  }
        public Player player { get; }
        public PlayerIdlingState idlingState { get; }
        public PlayerWalkingState walkingState { get; }
        public PlayerRunningState runningState { get; }
        public PlayerSprintingState sprintingState { get; }

        public PlayerDashingState dashingState { get;  }

        public PlayerDashBackingState dashBackingState { get; }

        public PlayerReturnRunState returnRunState{ get; }

        public PlayerOnSwitchState onSwitchState { get; }

        public PlayerOnSwitchOutState onSwitchOutState { get; }

        public PlayerMovementNullState playerMovementNullState { get; }

        public PlayerMovementStateMachine(Player P)
        {
            player = P;

            reusableDate =new PlayerStateReusableDate();
            //给状态传入该状态机的引用
           idlingState = new PlayerIdlingState(this);

            walkingState = new PlayerWalkingState(this);

            runningState = new PlayerRunningState(this);

            sprintingState = new PlayerSprintingState(this);

            dashingState = new PlayerDashingState(this);

            dashBackingState = new PlayerDashBackingState(this);

            returnRunState = new PlayerReturnRunState(this);

            onSwitchState=new PlayerOnSwitchState(this);

            onSwitchOutState=new PlayerOnSwitchOutState(this);

            playerMovementNullState=new PlayerMovementNullState(this);  

        }
            
    }
}
