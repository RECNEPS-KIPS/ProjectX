using System;
using ZZZ;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Mesh;
using System.Collections.Generic;

namespace ZZZ
{
    public class PlayerMovementState : IState
    {
        protected PlayerMovementStateMachine movementStateMachine { get; }
        protected Animator animator { get; }
        protected Transform playerTransform { get; }
        protected PlayerMovementData playerMovementData { get; }
        protected PlayerStateReusableDate reusableDate { get; }

        public PlayerMovementState(PlayerMovementStateMachine playerMovementStateMachine)
        {
            movementStateMachine = playerMovementStateMachine;
            if (playerTransform == null)
            {
                playerTransform = movementStateMachine.player.transform;
            }

            if (animator == null)
            {
                animator = movementStateMachine.player.characterAnimator;
            }

            if (playerMovementData == null)
            {
                playerMovementData = movementStateMachine.player.playerSO.movementData;
            }

            if (reusableDate == null)
            {
                reusableDate = movementStateMachine.reusableDate;
            }
        }

        public virtual void Enter()
        {
            AddInputActionCallBacks();
            Debug.Log(movementStateMachine.player.characterName + " GetType " + GetType().Name);
        }


        public virtual void Exit()
        {
            RemoveInputActionCallBacks();
        }

        public virtual void HandInput()
        {
            animator.SetFloat(AnimatorID.MovementID,
                GetPlayerMovementInputDirection().sqrMagnitude * reusableDate.inputMult, 0.35f, Time.deltaTime);
        }

        public virtual void Update()
        {
            CharacterRotation(GetPlayerMovementInputDirection());
        }

        public virtual void OnAnimationTranslateEvent(IState state)
        {
            movementStateMachine.ChangeState(state);
        }

        public virtual void OnAnimationExitEvent()
        {
        }

        protected Vector2 GetPlayerMovementInputDirection()
        {
            return CharacterInputSystem.MainInstance.PlayerMove;
        }

        float currentVelocity = 0;

        protected void CharacterRotation(Vector2 movementDirection)
        {
            if (GetPlayerMovementInputDirection() == Vector2.zero)
            {
                return;
            }

            reusableDate.targetAngle = Mathf.Atan2(movementDirection.x, movementDirection.y) * Mathf.Rad2Deg +
                                       movementStateMachine.player.camera.eulerAngles.y;

            movementStateMachine.player.transform.eulerAngles = Vector3.up *
                                                                Mathf.SmoothDampAngle(
                                                                    movementStateMachine.player.transform.eulerAngles.y,
                                                                    reusableDate.targetAngle, ref currentVelocity,
                                                                    reusableDate.rotationTime);

            // Vector3 targetDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

            //movementStateMachine.player.transform.rotation = Quaternion.lerp(movementStateMachine.player.transform.rotation,Quaternion.Euler(0, targetAngle,0),Time.deltaTime*20);
        }

        #region

        protected virtual void AddInputActionCallBacks()
        {
            CharacterInputSystem.MainInstance.inputActions.Player.Walk.started += OnWalkStart;
            CharacterInputSystem.MainInstance.inputActions.Player.Run.started += OnDashStart;
            // CharacterInputSystem.MainInstance.inputActions.Player.SwitchCharacter.started += OnSwitchCharacterStart;
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.canceled += OnMovementCanceled;
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.performed += OnMovementPerformed;
            CharacterInputSystem.MainInstance.inputActions.Player.CameraLook.started += OnMouseMovementStarted;
        }


        protected virtual void RemoveInputActionCallBacks()
        {
            CharacterInputSystem.MainInstance.inputActions.Player.Walk.started -= OnWalkStart;
            CharacterInputSystem.MainInstance.inputActions.Player.Run.started -= OnDashStart;
            // CharacterInputSystem.MainInstance.inputActions.Player.SwitchCharacter.started -= OnSwitchCharacterStart;
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.canceled -= OnMovementCanceled;
            CharacterInputSystem.MainInstance.inputActions.Player.Movement.performed -= OnMovementPerformed;
            CharacterInputSystem.MainInstance.inputActions.Player.CameraLook.started -= OnMouseMovementStarted;
        }

        #endregion

        protected virtual void OnWalkStart(InputAction.CallbackContext context)
        {
            Debug.Log(movementStateMachine.reusableDate.shouldWalk);
            reusableDate.shouldWalk = !reusableDate.shouldWalk;
            ;
        }

        private void OnDashStart(InputAction.CallbackContext context)
        {
            if (movementStateMachine.player.comboStateMachine.currentState.Value ==
                movementStateMachine.player.comboStateMachine.SkillState)
            {
                return;
            }

            if (reusableDate.canDash)
            {
                if (CharacterInputSystem.MainInstance.PlayerMove != Vector2.zero)
                {
                    animator.CrossFadeInFixedTime(playerMovementData.dashData.frontDushAnimationName,
                        playerMovementData.dashData.fadeTime);
                }
                else
                {
                    animator.CrossFadeInFixedTime(playerMovementData.dashData.backDushAnimationName,
                        playerMovementData.dashData.fadeTime);
                }
            }
        }

        // private void OnSwitchCharacterStart(InputAction.CallbackContext context)
        // {
        //     
        //     if (movementStateMachine.player.characterName == SwitchCharacter.MainInstance.newCharacterName.Value)
        //     {
        //       
        //         if (movementStateMachine.player.currentMovementState == "PlayerSprintingState")
        //         {
        //             movementStateMachine.player.CanSprintOnSwitch = true;
        //         }
        //         else
        //         {
        //             movementStateMachine.player.CanSprintOnSwitch = false;
        //         }
        //         SwitchCharacter.MainInstance.SwitchInput();
        //     }
        // }

        public virtual void ResetDash()
        {
            reusableDate.canDash = true;
        }

        #region

        public void UpdateCameraRecenteringState(Vector2 movementInput)
        {
            if (movementInput == Vector2.zero)
            {
                return;
            }

            if (movementInput == Vector2.up)
            {
                DisableCameraRecentering();
                return;
            }

            float cameraVerticalAngle = movementStateMachine.player.camera.localEulerAngles.x;

            if (cameraVerticalAngle > 270f)
            {
                cameraVerticalAngle -= 360f;
            }

            cameraVerticalAngle = Mathf.Abs(cameraVerticalAngle);

            if (movementInput == Vector2.down)
            {
                SetCameraRecentering(cameraVerticalAngle, playerMovementData.BackWardsCameraRecenteringData);
                return;
            }

            SetCameraRecentering(cameraVerticalAngle, playerMovementData.SidewaysCameraRecenteringData);
        }

        protected void SetCameraRecentering(float cameraVerticalAngle,
            List<PlayerCameraRecenteringData> playerCameraRecenteringDates)
        {
            foreach (PlayerCameraRecenteringData recenteringData in playerCameraRecenteringDates)
            {
                if (!recenteringData.IsWithInAngle(cameraVerticalAngle))
                {
                    continue;
                }

                EnableCameraRecentering(recenteringData.waitingTime, recenteringData.recenteringTime);
                return;
            }

            DisableCameraRecentering();
        }

        public void EnableCameraRecentering(float waitTime = -1f, float recenteringTime = -1)
        {
            movementStateMachine.player.playerCameraUtility.EnableRecentering(waitTime, recenteringTime);
        }

        public void DisableCameraRecentering()
        {
            movementStateMachine.player.playerCameraUtility.DisableRecentering();
        }

        private void OnMovementCanceled(InputAction.CallbackContext context)
        {
            DisableCameraRecentering();
        }

        private void OnMouseMovementStarted(InputAction.CallbackContext context)
        {
            UpdateCameraRecenteringState(GetPlayerMovementInputDirection());
        }

        private void OnMovementPerformed(InputAction.CallbackContext context)
        {
            UpdateCameraRecenteringState(context.ReadValue<Vector2>());
        }

        #endregion
    }
}