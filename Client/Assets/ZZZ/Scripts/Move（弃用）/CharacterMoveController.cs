using GGG.Tool;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Unity.VisualScripting.AnnotationUtility;
using static UnityEngine.InputSystem.InputSettings;

public enum PoseStation
{ 
    Stand,
    Crouch,
    MidAir,
    ATK,
    Aim
      
}
public enum MoveStation
{ 
    Idle,
    Walk,
    Run
}
public enum CharacterName
{ 
    HuoHuo,
    Anby,
    KeLin

}
//public class CharacterMoveController : CharacterMoveControllerBase
//{
//    //人物的旋转
//    //人物的PoseStation
//    //人物的MoveStation
//    //Animator数值设置
//    [SerializeField, Header("角色的名字")] private CharacterName characterName;
//    [SerializeField, Header("角色的当前转向")] private float currentRotationTime;
//    [SerializeField, Header("角色奔跑的转向")] private float runRotationTime;
//    [SerializeField, Header("角色idle的转向")] private float normalRotationTime;
//    private Transform cameraTransform;
//    private float rotationTargetAngle;
//    private float currentVelocity = 0;
//    private Vector3 characterTargetDir;
//    private float turnDeltaAngle;

//    public static PoseStation poseStation;
//    //private float standThreshold = 1;
//    //private float crouchThreshold = 0;
//    //private float midAirThreshold = 2.1f;
//    [SerializeField] MoveStation moveStation;
//    [SerializeField, Header("达到Run的时间")] private float walkToRunTimer=4;
//    private float walkToRunDeltaTimer;
//    [SerializeField, Header("达到Idle的缓冲时间")] private float toIdleBufferTimer=0.3f;
//    //private float toIdleBufferDeltaTimer;
//    //private bool hasDodge;
//    [SerializeField, Header("闪避的冷冻时间")] private float dodgeColdTime;

   

//    protected override void Awake()
//    {
//        base.Awake();
//        cameraTransform = Camera.main.transform;
//    }
//    protected override void Start()
//    {
//        hasDodge = false;
//        walkToRunDeltaTimer = walkToRunTimer;
//        toIdleBufferDeltaTimer = toIdleBufferTimer;
//    }
//    //protected override void Update()
//    //{
//    //    base.Update();
//    //    if (characterName== SwitchCharacter.newCharacterName.Value)
//    //    {
//    //        UpdateDodge();  
//    //        UpdatePoseStation();
//    //        SetPoseStationValue(); 
//    //        UpdateMoveStation();
//    //        SetAnimationValue();
           
//    //        UpdateRotationTime();
//    //    }
       
//    //}

    

//    private void LateUpdate()
//    {
//        CharacterRotation();
//    }
   
//    private bool CanRotation()
//    {
//        if (characterAnimator.AnimationAtTag("TurnRun")) return false;
//        return true;
//    }
//    /// <summary>
//    /// 更新旋转
//    /// </summary>
//    private void CharacterRotation()
//    {
//        if (poseStation == PoseStation.Aim) { return; }
//        if (characterAnimator.AnimationAtTag("TurnRun")&&characterAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime<0.3f)
//        { return; }
//        if ((characterAnimator.AnimationAtTag("ATK")||characterAnimator.AnimationAtTag("Skill")) && characterAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.25f)
//        { return; }
//        if (characterAnimator.AnimationAtTag("RushATK")) 
//        { return; }
        

//            //计算角色的旋转角
//         rotationTargetAngle = Mathf.Atan2(CharacterInputSystem.MainInstance.PlayerMove.x, CharacterInputSystem.MainInstance.PlayerMove.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
//        //得到目标期望转向的方向
//        //Quaternion.Euler*Vector3表示对这个向量进行旋转
//        characterTargetDir = Quaternion.Euler(0, rotationTargetAngle, 0) * Vector3.forward;
//        //计算目标方向与角色朝向的夹角    

//        turnDeltaAngle = DevelopmentToos.GetDeltaAngle(transform, characterTargetDir);
//        //if (characterAnimator.GetBool(AnimatorID.HasMoveInputID))
//        //{
           
//        //    characterAnimator.SetFloat(AnimatorID.TurnDeltaAngleID, turnDeltaAngle);

//        //    //把旋转量应用到角色身上
//        //    //if (!characterAnimator.AnimationAtTag("TurnRun"))
//        //    {
//        //        transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationTargetAngle, ref currentVelocity, currentRotationTime);
//        //    }
            
//        //}
        
//    }
//    private void UpdateRotationTime()
//    {
//        if (characterAnimator.GetBool(AnimatorID.RunID))
//        {
//            currentRotationTime = runRotationTime;
//        }
//        else
//        {
//            currentRotationTime = normalRotationTime;
//        }
//    }
//    /// <summary>
//    /// 更新Pose
//    /// </summary>
//    private void UpdatePoseStation()
//    {

//        if (!isOnGround)//在空中
//        {
//            poseStation = PoseStation.MidAir;

//            if (CharacterInputSystem.MainInstance.Crouch)//下降
//            {

//            }
//        }
//        else if (CharacterInputSystem.MainInstance.Crouch)
//        {
//            poseStation = PoseStation.Crouch;
//        }
//        else if (characterAnimator.AnimationAtTag("ATK") || characterAnimator.AnimationAtTag("RushATK") || characterAnimator.AnimationAtTag("Skill"))
//        {
//            poseStation = PoseStation.ATK;
//        }
//        else
//        {
//            poseStation = PoseStation.Stand;
//        }
//    }
    //private void SetPoseStationValue()
    //{
    //    switch (poseStation)
    //    {
    //        case PoseStation.MidAir:
    //            characterAnimator.SetFloat(AnimatorID.PoseStationID, midAirThreshold,0.15f,Time.deltaTime);
    //            break;
    //        case PoseStation.Crouch:
    //            characterAnimator.SetFloat(AnimatorID.PoseStationID, crouchThreshold, 0.25f, Time.deltaTime);
    //            break;
    //        case PoseStation.Stand:
    //            characterAnimator.SetFloat(AnimatorID.PoseStationID, standThreshold, 0.05f, Time.deltaTime);
    //            break;
    //        case PoseStation.ATK:
    //            characterAnimator.SetFloat(AnimatorID.PoseStationID, standThreshold, 0.05f, Time.deltaTime);
    //            moveStation = MoveStation.Idle;
    //            break;
         
            
    //    }
    //}
    /// <summary>
    /// 更新MoveStation的值
    /// </summary>
    //private void UpdateMoveStation()
    //{

    //    if (poseStation == PoseStation.Stand || poseStation == PoseStation.Aim)
    //    {
            
           
    //        if (characterAnimator.AnimationAtTag("Dodge"))
    //        {
    //            characterAnimator.SetBool(AnimatorID.HasInputForStopID, true);

    //            moveStation = MoveStation.Run;
    //        }
    //        else if (characterAnimator.GetBool(AnimatorID.HasMoveInputID))
    //        {
    //            toIdleBufferDeltaTimer = toIdleBufferTimer;
    //            characterAnimator.SetBool(AnimatorID.HasInputForStopID, true);

    //            if (moveStation != MoveStation.Run)
    //            {
    //                moveStation = MoveStation.Walk;
    //                walkToRunDeltaTimer -= Time.deltaTime;
    //                if (walkToRunDeltaTimer <= 0)
    //                {
    //                    walkToRunDeltaTimer = walkToRunTimer;
    //                    moveStation = MoveStation.Run;
    //                }

    //            }
    //        }
    //        else if (!characterAnimator.GetBool(AnimatorID.HasMoveInputID))
    //        {
    //            {
    //                toIdleBufferDeltaTimer -= Time.deltaTime;
    //                if (toIdleBufferDeltaTimer <= 0)
    //                {
    //                    characterAnimator.SetBool(AnimatorID.HasInputForStopID, false);
    //                    moveStation = MoveStation.Idle;
    //                    toIdleBufferDeltaTimer = toIdleBufferTimer;
    //                    walkToRunDeltaTimer = walkToRunTimer;

    //                }
    //            }

    //        }
    //    }
        

        
    //}

    #region 闪避系统
    //private void UpdateDodge()
    //{
    //    if (CharacterInputSystem.MainInstance.Run)
    //    {
    //        if (characterAnimator.GetBool(AnimatorID.HasMoveInputID))
    //        {
    //            //ToDo处理闪避

    //            ExecuteDodge();
    //        }
    //        else
    //        {
    //            //处理原地的闪避
    //           // moveStation = MoveStation.Run;
    //           // GameEventsManager.MainInstance.CallEvent("开启Idle冲刺攻击与输入");
    //            ExecuteDodgeInPlace();

    //        }
         
          

    //    }
    //}
    //private void ExecuteDodgeInPlace()
    //{
    //    if (!hasDodge)
    //    {
    //        characterAnimator.CrossFadeInFixedTime("Dodge_Back", 0.1f, 0);
         
    //        TimerManager.MainInstance.GetOneTimer(dodgeColdTime, ResetDodge);
    //        hasDodge = true;


    //    }
    //}
    //private void ExecuteDodge()
    //{
    //    if (!hasDodge)
    //    {
    //        characterAnimator.CrossFadeInFixedTime("Dodge_Front", 0.1f, 0);
    //        TimerManager.MainInstance.GetOneTimer(dodgeColdTime, ResetDodge);
    //        hasDodge = true;
    //    }
            
           


    //}

    #endregion

    //private void SetAnimationValue()
    //{
    //    characterAnimator.SetBool(AnimatorID.HasInputID, CharacterInputSystem.MainInstance.PlayerMove != Vector2.zero || poseStation == PoseStation.Crouch);
    //    characterAnimator.SetBool(AnimatorID.HasMoveInputID, CharacterInputSystem.MainInstance.PlayerMove != Vector2.zero);
    //    if (poseStation == PoseStation.Stand ||poseStation==PoseStation.Aim)
    //    {
    //        switch (moveStation)
    //        {
    //            case MoveStation.Idle:
    //                characterAnimator.SetFloat(AnimatorID.MovementID, 0, 0.1f, Time.deltaTime);
    //                if (characterAnimator.GetFloat(AnimatorID.MovementID) < 1.5)
    //                {
    //                    characterAnimator.SetBool(AnimatorID.RunID, false);
    //                }
    //                break;
    //            case MoveStation.Walk:
    //                characterAnimator.SetFloat(AnimatorID.MovementID,characterAnimator.GetBool(AnimatorID.RunID)? CharacterInputSystem.MainInstance.PlayerMove.sqrMagnitude * 3 : CharacterInputSystem.MainInstance.PlayerMove.sqrMagnitude * 2, 0.6f, Time.deltaTime);

    //                break;
    //            case MoveStation.Run:
    //                characterAnimator.SetBool(AnimatorID.RunID, true);
    //                characterAnimator.SetFloat(AnimatorID.MovementID, characterAnimator.GetBool(AnimatorID.RunID) ? CharacterInputSystem.MainInstance.PlayerMove.sqrMagnitude * 3 : CharacterInputSystem.MainInstance.PlayerMove.sqrMagnitude * 2, 0.5f, Time.deltaTime);
    //                break;
    //        }
    //    }
    //    else if (poseStation==PoseStation.ATK)
    //    {
    //        characterAnimator.SetFloat(AnimatorID.MovementID, 0, 0.1f, Time.deltaTime);
    //        if (characterAnimator.GetFloat(AnimatorID.MovementID) < 1)
    //        {
    //            characterAnimator.SetBool(AnimatorID.RunID, false);
    //        }
    //    }
    //    else if (poseStation == PoseStation.Crouch)
    //    {
    //        characterAnimator.SetFloat(AnimatorID.MovementID,  CharacterInputSystem.MainInstance.PlayerMove.sqrMagnitude * 2, 0.35f, Time.deltaTime);
    //        characterAnimator.SetBool(AnimatorID.RunID, false);
    //    }
    //}
   
    //private void ResetDodge()
    //{
    //    hasDodge = false;
    //}
   

    //public CharacterName CharacterName => characterName;
   
//}
