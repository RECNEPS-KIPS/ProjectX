using System;
using System.Collections;
using System.Collections.Generic;
using GameLogic;
using UnityEngine;

namespace GameFramework
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMoveComponent3D : MonoBehaviour,ICustomInitAndRelease,IActivatable
    {
        // playerData
        // Config
        [SerializeField]
        private float m_walkSpeed = 3f;
        [SerializeField]
        private float m_runSpeed = 5f;
        [SerializeField]
        private float m_jumpHeight = 2f;
        [SerializeField] 
        private float m_gravityAddSpeed = 9.8f;
        [SerializeField] 
        private Transform m_characterCameraTrans;
        [SerializeField] private float m_rotateSpeed = 5f;
        
        // Logic 
        [SerializeField]
        private Vector3 m_moveDirection;
        [SerializeField] 
        private Vector3 m_verticalVelocity;
        [SerializeField]
        private bool m_isGround;
        [SerializeField] 
        private bool m_isFalling;
        [SerializeField] 
        private bool m_isJump = false;
        
        // playerConfig
        [SerializeField]
        private float m_groundSensorRadius = 0.2f;
        [SerializeField] 
        private Vector3 m_groundSensorOffset = new Vector3(0, -1, 0);
        
        private GroundSensor m_groudSensor;
        private CharacterController m_characterController;
        private GameInputModule m_gameInputModel;
        private GameRuntimeDataModule m_gameRuntimeDataModule;
        
#if UNITY_EDITOR
        [Header("===== Debug ====")]
        [SerializeField]
        private bool isDebugGroundSensor;
        [SerializeField] 
        private bool isDebugForward;
#endif
        
            
        public bool IsActive { get; set; }
        public void CustomInit()
        {
            // module
            m_gameInputModel = GameRoot.Instance.GetModule<GameInputModule>();
            m_gameRuntimeDataModule = GameRoot.Instance.GetModule<GameRuntimeDataModule>();
            // sensor
            m_groudSensor = new GroundSensor();
            m_characterController = GetComponent<CharacterController>();
        }

        public void CustomRelease()
        {
            
        }

        public void MoveUpdate()
        {
            if (!IsActive)
                return;
            // SensorCheck
            m_isGround = m_groudSensor.IsGround(transform.position + m_groundSensorOffset,m_groundSensorRadius);
            // InputValid
            if (!m_gameInputModel.IsInputActive(eInputModel.PlayerControllerInput))
                return;
            m_moveDirection = m_gameInputModel.GetNormalLeftJoyStickValue();
            var faceForward = transform.forward;
            // Rotate
            if (m_moveDirection.magnitude > 0)
            {
                var viewDir = (transform.position - m_characterCameraTrans.transform.position).SetYZero();
                transform.rotation = Quaternion.Slerp(Quaternion.LookRotation(faceForward), Quaternion.LookRotation(viewDir),m_rotateSpeed* Time.deltaTime);
            }
            // Move
            m_isJump = m_isGround & m_gameInputModel.GetJumpKeyPressed();
            m_isFalling = !m_isGround & m_characterController.velocity.y < 0;
            var moveVelocity = (faceForward * m_moveDirection.y + transform.right * m_moveDirection.x) * m_walkSpeed;
            // gravity
            var verticalVelocity = GetVerticalVelocity();
            moveVelocity += verticalVelocity;
            m_characterController.Move(moveVelocity * Time.deltaTime);
        }

        private Vector3 GetVerticalVelocity()
        {
            if (m_isGround && !m_isJump && !m_isFalling)
            {
                return m_verticalVelocity = Vector3.zero;
            }

            if (m_isJump)
            {
                var jumpVelocity = Mathf.Sqrt(2 * m_gravityAddSpeed * m_jumpHeight);
                m_verticalVelocity = new Vector3(0, jumpVelocity, 0);
            }
            m_verticalVelocity = new Vector3(0, m_verticalVelocity.y -= m_gravityAddSpeed * Time.deltaTime, 0);
            
            return m_verticalVelocity;
        }

        private void OnDrawGizmos()
        {
            if (isDebugGroundSensor)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position + m_groundSensorOffset,m_groundSensorRadius);
            }

            if (isDebugForward)
            {
                Gizmos.color = Color.red;
                var startPos = transform.position;
                Gizmos.DrawLine(startPos,startPos+transform.forward);
            }

        }
    }
}