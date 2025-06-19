using GGG.Tool;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZZZ
{
    public class CharacterCombo : CharacterComboBase
    {
        public CharacterCombo(Animator animator, Transform playerTransform, Transform cameraTransform,
            PlayerComboReusableData reusableData, PlayerComboSOData playerComboSOData,
            PlayerEnemyDetectionData playerEnemyDetectionData, Player player) : base(animator, playerTransform,
            cameraTransform, reusableData, playerComboSOData, playerEnemyDetectionData, player)
        {
        }

        #region

        public void DodgeComboInput()
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


        #region

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool CanFinishSkillInput()
        {
            if (animator.AnimationAtTag("Skill"))
            {
                return false;
            }

            if (animator.AnimationAtTag("Hit"))
            {
                return false;
            }

            if (animator.AnimationAtTag("Parry"))
            {
                return false;
            }

            if (animator.AnimationAtTag("ATK"))
            {
                return false;
            }

            if (comboData.finishSkillCombo == null)
            {
                return false;
            }

            return true;
        }

        public bool CanSkillInput()
        {
            if (animator.AnimationAtTag("Skill"))
            {
                return false;
            }

            if (animator.AnimationAtTag("Hit"))
            {
                return false;
            }

            if (animator.AnimationAtTag("Parry"))
            {
                return false;
            }

            if (animator.AnimationAtTag("ATK"))
            {
                return false;
            }

            if (comboData.skillCombo == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///
        /// </summary>
        public void FinishSkillInput()
        {
            if (comboData.finishSkillCombo == null)
            {
                return;
            }

            if (reusableData.currentCombo == null || reusableData.currentCombo != comboData.finishSkillCombo)
            {
                reusableData.currentSkill = comboData.finishSkillCombo;
            }

            ExecuteSkill();
        }

        /// <summary>
        ///
        /// </summary>
        public void SkillInput()
        {
            if (comboData.skillCombo == null)
            {
                return;
            }

            if (reusableData.currentCombo == null || reusableData.currentCombo != comboData.skillCombo)
            {
                reusableData.currentSkill = comboData.skillCombo;
            }

            ExecuteSkill();
        }

        /// <summary>
        ///
        /// </summary>
        private void ExecuteSkill()
        {
            ReSetATKIndex(0);

            PlayCharacterVoice(reusableData.currentSkill);

            PlayWeaponSound(reusableData.currentSkill);
            animator.CrossFadeInFixedTime(reusableData.currentSkill.comboName, 0.1f);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="attacker"></param>
        protected override void CanSwitchSkill(Transform transform)
        {
            if (playerTransform != transform)
            {
                return;
            }

            reusableData.canQTE = true;
        }

        protected override void TriggerSwitchSkill()
        {
            CharacterInputSystem.MainInstance.inputActions.Player.Disable();
            ReSetComboInfo();
            reusableData.canQTE = false;
            ZZZZTimerManager.MainInstance.GetOneTimer(0.3f, startSlowTime);
        }

        protected void startSlowTime()
        {
            SFX_PoolManager.MainInstance.TryGetSoundPool(SoundStyle.SwitchTime, player.characterName.ToString(),
                playerTransform.position);
            CameraHitFeel.MainInstance.StartSlowTime(0.06f);
            ZZZZTimerManager.MainInstance.GetRealTimer(0.2f, StartSwitchSkill);
        }

        protected void StartSwitchSkill()
        {
            ZZZZTimerManager.MainInstance.GetRealTimer(3, CancelSwitchSkill);
            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.L.started += SwitchL;
            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.R.started += SwitchR;
        }

        protected void CancelSwitchSkill()
        {
            CameraHitFeel.MainInstance.EndSlowTime();

            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.L.started -= SwitchL;
            CharacterInputSystem.MainInstance.inputActions.SwitchSkill.R.started -= SwitchR;

            CharacterInputSystem.MainInstance.inputActions.Player.Enable();
        }

        private void SwitchR(InputAction.CallbackContext context)
        {
        }

        private void SwitchL(InputAction.CallbackContext context)
        {
        }

        public void SwitchSkill(CharacterNameList characterName)
        {
            CameraHitFeel.MainInstance.EndSlowTime();
            reusableData.currentSkill = comboData.switchSkill;
            PlayCharacterVoice(reusableData.currentSkill);
            PlayWeaponSound(reusableData.currentSkill);
            CharacterInputSystem.MainInstance.inputActions.Player.Enable();
        }

        #endregion

        #region

        public void UpdateDetectionDir()
        {
            Vector3 camForwardDir = Vector3.zero;
            camForwardDir.Set(reusableData.cameraTransform.forward.x, 0, reusableData.cameraTransform.forward.z);
            camForwardDir.Normalize();

            reusableData.detectionDir = camForwardDir * CharacterInputSystem.MainInstance.PlayerMove.y +
                                        reusableData.cameraTransform.right *
                                        CharacterInputSystem.MainInstance.PlayerMove.x;
            reusableData.detectionDir.Normalize();
        }

        public void UpdateEnemy()
        {
            UpdateDetectionDir();

            reusableData.detectionOrigin = new Vector3(playerTransform.position.x, playerTransform.position.y + 0.7f,
                playerTransform.position.z);

            if (Physics.SphereCast(reusableData.detectionOrigin, enemyDetectionData.detectionRadius,
                    reusableData.detectionDir, out var hit, enemyDetectionData.detectionLength,
                    enemyDetectionData.WhatIsEnemy))
            {
                if (GameBlackboard.MainInstance.GetEnemy() != hit.collider.transform ||
                    GameBlackboard.MainInstance.GetEnemy() == null)
                {
                    GameBlackboard.MainInstance.SetEnemy(hit.collider.transform);
                }
            }
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(
                reusableData.detectionOrigin + reusableData.detectionDir * enemyDetectionData.detectionLength,
                enemyDetectionData.detectionRadius);
        }

        #endregion
    }
}