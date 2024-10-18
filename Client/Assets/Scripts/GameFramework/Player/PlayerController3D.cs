using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class PlayerControllerData
    {
        public bool CanJump { get; set; }
        public bool CanRun { get; set; }
        public Vector3 MoveDirection { get; set; }
    }
    
    public class PlayerController3D : MonoBehaviour
    {
        [SerializeField]
        private CharacterMoveComponent3D m_moveComponent3D;
        [SerializeField]
        private PlayerCameraController3D m_cameraController3D;
        private void Start()
        {
            m_moveComponent3D.CustomInit();
            m_moveComponent3D.IsActive = true;
            
            m_cameraController3D.CustomInit();
            m_cameraController3D.IsActive = true;
        }

        private void Update()
        {
            m_moveComponent3D.MoveUpdate();
        }
    }
}