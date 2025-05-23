using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorID : MonoBehaviour
{
    public static readonly int MovementID = Animator.StringToHash("Movement");
    public static readonly int RunID = Animator.StringToHash("Run");
    public static readonly int HasInputID = Animator.StringToHash("HasInput");
    public static readonly int HorizontalID = Animator.StringToHash("Horizontal");
    public static readonly int VerticalID = Animator.StringToHash("Vertical");
    public static readonly int LockID = Animator.StringToHash("Lock");
    public static readonly int AimID = Animator.StringToHash("Aim");
    public static readonly int TurnBackID = Animator.StringToHash("TurnBack");

}
