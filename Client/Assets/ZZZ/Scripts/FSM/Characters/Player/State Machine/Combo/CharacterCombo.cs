using GGG.Tool;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZZZ
{
    public class CharacterCombo : CharacterComboBase
    {

        public CharacterCombo(Animator animator, Transform playerTransform,Transform cameraTransform, PlayerComboReusableData reusableData, PlayerComboSOData playerComboSOData, PlayerEnemyDetectionData playerEnemyDetectionData,Player player) : base(animator, playerTransform, cameraTransform, reusableData, playerComboSOData, playerEnemyDetectionData , player )
        {
           
        }

        #region 闪A处理
       

        public  void DodgeComboInput()
        {
            switch (SwitchCharacter.MainInstance.newCharacterName.Value)
            {
                case CharacterNameList.KeLin:
                    {
                        NormalDodgeCombo();
                    }
                    break;
                case CharacterNameList.NiKe:
                    {
                        NormalDodgeCombo();
                    }
                    break;
                case CharacterNameList.BiLi:
                    {
                        NormalDodgeCombo();
                    }
                    break;
                case CharacterNameList.AnBi:
                    {
                        NormalDodgeCombo();
                    }
                    break;
            }
        }

        #endregion

        #region 处决处理
       
        #endregion

        #region 技能处理

        /// <summary>
        /// 主动技能
        /// </summary>
        /// <returns></returns>
        public bool CanFinishSkillInput()
        {
            if (animator.AnimationAtTag("Skill")) { return false; }
            if (animator.AnimationAtTag("Hit")) { return false; }
            if (animator.AnimationAtTag("Parry")) { return false; }
            if (animator.AnimationAtTag("ATK")) { return false; }
            if (comboData.finishSkillCombo == null) { return false; }
         
            return true;

        }
        public bool CanSkillInput()
        {
            if (animator.AnimationAtTag("Skill")) { return false; }
            if (animator.AnimationAtTag("Hit")) { return false; }
            if (animator.AnimationAtTag("Parry")) { return false; }
            if (animator.AnimationAtTag("ATK")) { return false; }
            if (comboData.skillCombo == null) { return false; }

            return true;

        }

        /// <summary>
        /// 终极大招
        /// </summary>
        public void FinishSkillInput()
        {
            if (comboData.finishSkillCombo == null) { return; }
            if (reusableData.currentCombo == null || reusableData.currentCombo != comboData.finishSkillCombo)
            {
                reusableData.currentSkill = comboData.finishSkillCombo;
            }
            ExecuteSkill();
        }
        /// <summary>
        /// 大招
        /// </summary>
        public void SkillInput()
        {
            if (comboData.skillCombo == null) { return; }
            if (reusableData.currentCombo == null || reusableData.currentCombo != comboData.skillCombo)
            {
                reusableData.currentSkill = comboData.skillCombo;
            }
            ExecuteSkill();
        }
       
        /// <summary>
        /// 执行大招
        /// </summary>
        private void ExecuteSkill()
        {
            ReSetATKIndex(0);
            //播放语音
            PlayCharacterVoice(reusableData.currentSkill);
            //播放武器音效
            PlayWeaponSound(reusableData.currentSkill);
            animator.CrossFadeInFixedTime(reusableData.currentSkill.comboName, 0.1f);
        }

        /// <summary>
        /// 被动技能:等待选择切换的角色
        /// </summary>
        /// <param name="attacker"></param>
        protected override void CanSwitchSkill(Transform transform)
        {
            if (playerTransform != transform) { return; }
             reusableData.canQTE = true;
        }
        protected override void TriggerSwitchSkill()
        {
            //禁用人物所有输入
            CharacterInputSystem.MainInstance.inputActions.Player.Disable();
            //重置玩家连招
            ReSetComboInfo();
            //激活切人相机
            CameraSwitcher.MainInstance.ActiveSwitchCamera(true);   
            //注册输入切人事件
            reusableData.canQTE = false;
            //留0.5的时间然后放慢是因为镜头要切换到位
            ZZZTimerManager.MainInstance.GetOneTimer(0.3f, startSlowTime);

        }
        protected void startSlowTime()
        {
            //放慢时间，但是还不能输入
            SFX_PoolManager.MainInstance.TryGetSoundPool(SoundStyle.SwitchTime,player.characterName.ToString(),playerTransform.position); 
            CameraHitFeel.MainInstance.StartSlowTime(0.06f);
            ZZZTimerManager.MainInstance.GetRealTimer(0.2f, StartSwitchSkill);
        }
        protected void StartSwitchSkill()
        {
            //QTE倒计时
            ZZZTimerManager.MainInstance.GetRealTimer(3, CancelSwitchSkill);
            //激活UI
            ZZZUIManager.MainInstance.switchTimeUI.ActiveImage(SwitchCharacter.MainInstance.waitingCharacterList[0], SwitchCharacter.MainInstance.waitingCharacterList[1],3);
            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.L.started += SwitchL;
            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.R.started += SwitchR;
        }
        protected void CancelSwitchSkill()
        {
            //恢复时间
            CameraHitFeel.MainInstance.EndSlowTime();
            //恢复镜头
            CameraSwitcher.MainInstance.ActiveSwitchCamera(false);
            //关闭UI
            ZZZUIManager.MainInstance.switchTimeUI.UnActive();
            //注销输入
            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.L.started -= SwitchL;
            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.R.started -= SwitchR;
            //恢复输入
            CharacterInputSystem.MainInstance.inputActions.Player.Enable();
        }

        private void SwitchR(InputAction.CallbackContext context)
        {
            //选择切人的角色
            CharacterNameList selectCharacter = SwitchCharacter.MainInstance.waitingCharacterList[1];
            //通知对象你要触发技能：通过黑板模式通知
            GameBlackboard.MainInstance.GetGameData<Player>(selectCharacter.ToString()).comboStateMachine.ATKIngState.SwitchSkill();
            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.L.started -= SwitchL;
            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.R.started -= SwitchR;
        }

        private void SwitchL(InputAction.CallbackContext context)
        {
            CharacterNameList selectCharacter = SwitchCharacter.MainInstance.waitingCharacterList[0];
            //通知对象你要触发技能：通过黑板模式通知
            GameBlackboard.MainInstance.GetGameData<Player>(selectCharacter.ToString()).comboStateMachine.ATKIngState.SwitchSkill();
            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.L.started -= SwitchL;
            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.R.started -= SwitchR;
        }
        public void SwitchSkill(CharacterNameList characterName)
        {
            //关闭UI
            ZZZUIManager.MainInstance.switchTimeUI.UnActive();
            //恢复时间
            CameraHitFeel.MainInstance.EndSlowTime();
            //结束切人相机
            CameraSwitcher.MainInstance.ActiveSwitchCamera(false);
            //改技能
            reusableData.currentSkill = comboData.switchSkill;
            //播放切人动画,//通知切换角色管理器处理切换技能
            SwitchCharacter.MainInstance.SwitchSkillInput(characterName, reusableData.currentSkill.comboName);
            //播放语音
            PlayCharacterVoice(reusableData.currentSkill);
            //播放武器音效
            PlayWeaponSound(reusableData.currentSkill);
            //恢复输入
            CharacterInputSystem.MainInstance.inputActions.Player.Enable();
           


        }

        #endregion

        #region 敌人检测
        public void UpdateDetectionDir()
        {
         
            Vector3 camForwardDir = Vector3.zero;
            camForwardDir.Set(reusableData.cameraTransform.forward.x, 0, reusableData.cameraTransform.forward.z);
            camForwardDir.Normalize();
         
           reusableData.detectionDir = camForwardDir * CharacterInputSystem.MainInstance.PlayerMove.y + reusableData.cameraTransform.right * CharacterInputSystem.MainInstance.PlayerMove.x;
           reusableData.detectionDir.Normalize();
        }
        public void UpdateEnemy()
        {
            UpdateDetectionDir();

            reusableData.detectionOrigin = new Vector3(playerTransform.position.x, playerTransform.position.y + 0.7f, playerTransform.position.z);
           
            if (Physics.SphereCast(reusableData.detectionOrigin,enemyDetectionData.detectionRadius, reusableData.detectionDir, out var hit, enemyDetectionData.detectionLength, enemyDetectionData.WhatIsEnemy))
            {
                if (GameBlackboard.MainInstance.GetEnemy() != hit.collider.transform || GameBlackboard.MainInstance.GetEnemy() == null)
                {   
                    GameBlackboard.MainInstance.SetEnemy(hit.collider.transform);
                }
            }
        }
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(reusableData. detectionOrigin + reusableData.detectionDir * enemyDetectionData.detectionLength, enemyDetectionData.detectionRadius);
        }

        #endregion


    }
}
