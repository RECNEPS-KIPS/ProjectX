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

        #region ��A����
       

        public  void DodgeComboInput()
        {
            // switch (SwitchCharacter.MainInstance.newCharacterName.Value)
            // {
            //     case CharacterNameList.KeLin:
            //         {
            //             NormalDodgeCombo();
            //         }
            //         break;
            //     case CharacterNameList.NiKe:
            //         {
            //             NormalDodgeCombo();
            //         }
            //         break;
            //     case CharacterNameList.BiLi:
            //         {
            //             NormalDodgeCombo();
            //         }
            //         break;
            //     case CharacterNameList.AnBi:
            //         {
            //             NormalDodgeCombo();
            //         }
            //         break;
            // }
        }

        #endregion

        #region ��������
       
        #endregion

        #region ���ܴ���

        /// <summary>
        /// ��������
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
        /// �ռ�����
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
        /// ����
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
        /// ִ�д���
        /// </summary>
        private void ExecuteSkill()
        {
            ReSetATKIndex(0);
            //��������
            PlayCharacterVoice(reusableData.currentSkill);
            //����������Ч
            PlayWeaponSound(reusableData.currentSkill);
            animator.CrossFadeInFixedTime(reusableData.currentSkill.comboName, 0.1f);
        }

        /// <summary>
        /// ��������:�ȴ�ѡ���л��Ľ�ɫ
        /// </summary>
        /// <param name="attacker"></param>
        protected override void CanSwitchSkill(Transform transform)
        {
            if (playerTransform != transform) { return; }
             reusableData.canQTE = true;
        }
        protected override void TriggerSwitchSkill()
        {
            //����������������
            CharacterInputSystem.MainInstance.inputActions.Player.Disable();
            //�����������
            ReSetComboInfo();
            //�����������
            CameraSwitcher.MainInstance.ActiveSwitchCamera(true);   
            //ע�����������¼�
            reusableData.canQTE = false;
            //��0.5��ʱ��Ȼ���������Ϊ��ͷҪ�л���λ
            ZZZZTimerManager.MainInstance.GetOneTimer(0.3f, startSlowTime);

        }
        protected void startSlowTime()
        {
            //����ʱ�䣬���ǻ���������
            SFX_PoolManager.MainInstance.TryGetSoundPool(SoundStyle.SwitchTime,player.characterName.ToString(),playerTransform.position); 
            CameraHitFeel.MainInstance.StartSlowTime(0.06f);
            ZZZZTimerManager.MainInstance.GetRealTimer(0.2f, StartSwitchSkill);
        }
        protected void StartSwitchSkill()
        {
            //QTE����ʱ
            ZZZZTimerManager.MainInstance.GetRealTimer(3, CancelSwitchSkill);
            //����UI
            // ZZZZUIManager.MainInstance.switchTimeUI.ActiveImage(SwitchCharacter.MainInstance.waitingCharacterList[0], SwitchCharacter.MainInstance.waitingCharacterList[1],3);
            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.L.started += SwitchL;
            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.R.started += SwitchR;
        }
        protected void CancelSwitchSkill()
        {
            //�ָ�ʱ��
            CameraHitFeel.MainInstance.EndSlowTime();
            //�ָ���ͷ
            CameraSwitcher.MainInstance.ActiveSwitchCamera(false);
            //�ر�UI
            ZZZZUIManager.MainInstance.switchTimeUI.UnActive();
            //ע������
            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.L.started -= SwitchL;
            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.R.started -= SwitchR;
            //�ָ�����
            CharacterInputSystem.MainInstance.inputActions.Player.Enable();
        }

        private void SwitchR(InputAction.CallbackContext context)
        {
            //ѡ�����˵Ľ�ɫ
            // CharacterNameList selectCharacter = SwitchCharacter.MainInstance.waitingCharacterList[1];
            // //֪ͨ������Ҫ�������ܣ�ͨ���ڰ�ģʽ֪ͨ
            // GameBlackboard.MainInstance.GetGameData<Player>(selectCharacter.ToString()).comboStateMachine.ATKIngState.SwitchSkill();
            // CharacterInputSystem.MainInstance.inputActions.SwitchSkill.L.started -= SwitchL;
            // CharacterInputSystem.MainInstance.inputActions.SwitchSkill.R.started -= SwitchR;
        }

        private void SwitchL(InputAction.CallbackContext context)
        {
            // CharacterNameList selectCharacter = SwitchCharacter.MainInstance.waitingCharacterList[0];
            // //֪ͨ������Ҫ�������ܣ�ͨ���ڰ�ģʽ֪ͨ
            // GameBlackboard.MainInstance.GetGameData<Player>(selectCharacter.ToString()).comboStateMachine.ATKIngState.SwitchSkill();
            // CharacterInputSystem.MainInstance.inputActions.SwitchSkill.L.started -= SwitchL;
            // CharacterInputSystem.MainInstance.inputActions.SwitchSkill.R.started -= SwitchR;
        }
        public void SwitchSkill(CharacterNameList characterName)
        {
            //�ر�UI
            ZZZZUIManager.MainInstance.switchTimeUI.UnActive();
            //�ָ�ʱ��
            CameraHitFeel.MainInstance.EndSlowTime();
            //�����������
            CameraSwitcher.MainInstance.ActiveSwitchCamera(false);
            //�ļ���
            reusableData.currentSkill = comboData.switchSkill;
            //�������˶���,//֪ͨ�л���ɫ�����������л�����
            // SwitchCharacter.MainInstance.SwitchSkillInput(characterName, reusableData.currentSkill.comboName);
            //��������
            PlayCharacterVoice(reusableData.currentSkill);
            //����������Ч
            PlayWeaponSound(reusableData.currentSkill);
            //�ָ�����
            CharacterInputSystem.MainInstance.inputActions.Player.Enable();
           


        }

        #endregion

        #region ���˼��
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
