using GGG.Tool;
using UnityEngine;


//public class CharacterCombo : CharacterComboBase
//{
//    [SerializeField, Header("敌人检测")] private float detectionRadius;
//    [SerializeField] private float detectionLength;
//    [SerializeField] private LayerMask WhatIsEnemy;
//    private Vector3 detectionDir;

//    private Vector3 detectionOrigin;
//    private Transform camTransform;
//    private void OnEnable()
//    {
      
//    }
//    private void OnDisable()
//    {
       
//    }

//    protected override void Awake()
//    {
//        base.Awake();
//        camTransform = Camera.main.transform;
//    }
//    protected override void Start()
//    {
//        base.Start();

//    }
//    protected override void Update()
//    {
//        base.Update();
//        BaseComboInput();
//        UpdateComboAnimation();
//        UpdateEnemy();
//        UpdateDetectionDir();
//        SkillInput();
//    }

//    protected override void BaseComboInput()
//    {
//        base.BaseComboInput();
//    }
  

//    #region 处决处理
//    protected bool CanSpecialATKInput()
//    {
//        if (animator.AnimationAtTag("Skill")) { return false; }
//        if (animator.AnimationAtTag("Execute")) { return false; }
//        if (animator.AnimationAtTag("Hit")) { return false; }
//        if (animator.AnimationAtTag("Parry")) { return false; }
//        if (animator.AnimationAtTag("ATK")) { return false; }
//        return true;
//    }
//    protected override void ExecuteInput()
//    {
//        if (!CanSpecialATKInput()) { return; }
//        if (CharacterInputSystem.MainInstance.Execute)
//        {
//            ExecuteSpecialATK();
//        }

//    }
//    protected override void ExecuteSpecialATK()
//    {
//        //TODO判断是否满足条件
//        if (AttackDetection(executeCombo)) { return; }
//      //  executeIndex = UpdateExecuteIndex(executeCombo);
//        //TODO 执行动画
//        //TODO 冷却时间
//        // hasExecute = true;

//    }
//    #endregion

//    #region 技能处理

//    private bool CanSkillInput()
//    {
//        if (animator.AnimationAtTag("Skill")) { return false; }
//        if (animator.AnimationAtTag("Execute")) { return false; }
//        if (animator.AnimationAtTag("Hit")) { return false; }
//        if (animator.AnimationAtTag("Parry")) { return false; }
//        if (animator.AnimationAtTag("ATK")) { return false; }
//        if (finishSkillCombo == null) { return false; }
//        if (skillCombo == null) { return false; }
//        return true;

//    }
//    private void SkillInput()
//    {
//        if (CharacterInputSystem.MainInstance.FinishSkill)
//        {
            
//            if (!CanSkillInput()) { return; }
//            if (currentCombo == null || currentCombo != finishSkillCombo)
//            {
//                currentComboData = finishSkillCombo;
//            }
//            ExecuteSkill();
//        }
//        if (CharacterInputSystem.MainInstance.Skill)
//        {
//            if (!CanSkillInput()) { return; }
//            if (currentCombo == null || currentCombo != skillCombo)
//            {
//                currentComboData = skillCombo;
//            }
//            ExecuteSkill();
//        }

//    }

//    private void ExecuteSkill()
//    {
//        animator.CrossFadeInFixedTime(currentComboData.comboName, 0.1f);
//    }




//    #endregion

//    #region 敌人检测
//    private void UpdateDetectionDir()
//    {
//        Vector3 camForwardDir = Vector3.zero;
//        camForwardDir.Set(camTransform.forward.x, 0, camTransform.forward.z);
//        camForwardDir.Normalize();

//        detectionDir = camForwardDir * CharacterInputSystem.MainInstance.PlayerMove.y + camTransform.right * CharacterInputSystem.MainInstance.PlayerMove.x;
//        detectionDir.Normalize();
//    }
//    private void UpdateEnemy()
//    {
//        detectionOrigin = new Vector3(transform.position.x, transform.position.y + 0.7f, transform.position.z);
//        //Physics.SphereCast(transform.position + (transform.up * 0.7f), _detectionRang, _detectionDirection, out var hit, _detectionLength, whatIsEnemy, QueryTriggerInteraction.Ignore))
//        if (Physics.SphereCast(detectionOrigin, detectionRadius, detectionDir, out var hit, detectionLength, WhatIsEnemy, QueryTriggerInteraction.Ignore))
//        {
//            // Debug.Log(LayerMaskNum);
//            if (enemy != hit.collider.transform || enemy == null)
//            {
//                enemy = hit.collider.transform;
//            }

//        }
//    }
//    private void OnDrawGizmos()
//    {
//        Gizmos.color = Color.white;
//        Gizmos.DrawWireSphere(detectionOrigin + detectionDir * detectionLength, detectionRadius);
//    }

//    #endregion
 

//}
