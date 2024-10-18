using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public enum eInputModel
    {
        None = 0,
        PlayerControllerInput = 1 << 0,
        UIInput = 1 << 1,
    }
    
    public class GameInputModule : GameFrameworkModule
    {
        private int m_inputModelValue;

        private float m_horizontalInput;
        private float m_verticalInput;
        private Vector2 m_leftJoyStickValue;
        private float m_viewHorizontalInput;
        private float m_viewVerticalInput;
        private Vector2 m_rightJoyStickValue;

        private bool m_isJumpKeyPressed;

        public Vector2 GetNormalLeftJoyStickValue()
        {
            return m_leftJoyStickValue.sqrMagnitude > 2 ?  m_leftJoyStickValue.normalized :new Vector2(m_horizontalInput, m_verticalInput);
        }
        
        public Vector2 GetNormalRightJoyStickValue()
        {
            return m_rightJoyStickValue.sqrMagnitude > 2 ?  m_rightJoyStickValue.normalized :new Vector2(m_viewHorizontalInput, m_viewVerticalInput);
        }

        public bool GetJumpKeyPressed()
        {
            return m_isJumpKeyPressed;
        }
        
        public GameInputModule()
        {
            
        }

        public void SetInputModel(eInputModel inputModel,bool isActive)
        {
            if (isActive)
            {
                m_inputModelValue |= (int)inputModel;
            }
            else
            {
                m_inputModelValue &= ~(int)inputModel;
            }
        }

        public bool IsInputActive(eInputModel inputModel)
        {
            return (m_inputModelValue & (int)inputModel) == (int)inputModel;
        }

        public void Update()
        {
            m_horizontalInput = Input.GetAxisRaw("Horizontal");
            m_verticalInput = Input.GetAxisRaw("Vertical");
            m_leftJoyStickValue = new Vector2(m_horizontalInput, m_verticalInput);
            m_viewHorizontalInput = Input.GetAxisRaw("Mouse X");
            m_viewVerticalInput = Input.GetAxisRaw("Mouse Y");
            m_rightJoyStickValue = new Vector2(m_viewHorizontalInput, m_viewVerticalInput);

            m_isJumpKeyPressed = Input.GetKeyDown(KeyCode.Space);
        }
    }
}