using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace GameFramework
{
    public class PlayerCameraController3D : MonoBehaviour,ICustomInitAndRelease,IActivatable
    {
        [SerializeField] private CinemachineFreeLook m_cinemachineFreeLook;
        public bool IsActive
        {
            get => m_cinemachineFreeLook.enabled;
            set => m_cinemachineFreeLook.enabled = value;
        }
        
        public void CustomInit()
        {
            // TODO 对接镜头移动输入
            m_cinemachineFreeLook.gameObject.transform.SetParent(null);
        }

        public void CustomRelease()
        {
            
        }
        
    }
}
