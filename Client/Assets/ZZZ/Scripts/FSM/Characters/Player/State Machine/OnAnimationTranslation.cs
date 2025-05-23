using System.Collections;
using System.Collections.Generic;
using ZZZ;
using UnityEngine;

public class OnAnimationTranslation : StateMachineBehaviour
{
      Player player;
    public enum OnEnterAnimationPlayerState
    { 
     Idle,
     Walk,
     Run,
     Sprint,
     Dash,
     DashBack,
     TurnBack,
     Switch,
     SwitchOut,
     ATK,
     Null
    
    }
   [SerializeField] public OnEnterAnimationPlayerState onEnterAnimationState;
  
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (onEnterAnimationState == OnEnterAnimationPlayerState.Null)
        {
            return;
        }
        if (animator.TryGetComponent<Player>(out player))
        {
            player.OnAnimationTranslateEvent(onEnterAnimationState);
        }
    }

   
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent<Player>(out player))
        {
            player.OnAnimationExitEvent();
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
