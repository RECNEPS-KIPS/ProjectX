using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerCameraUtility
{
    [field: SerializeField] public CinemachineVirtualCamera virtualCamera { get; private set; }
    [field: SerializeField] public float DefaultHorizontalWaitTime { get; private set; } = 0f;

    [field: SerializeField] public float DefaultHorizontalRecenteringTime { get; private set; } = 0.5f;

    private CinemachinePOV cinemachinePOV;

    public void EnableRecentering(float waitTime = -1f, float recenteringTime = -1f)
    {
        cinemachinePOV.m_HorizontalRecentering.m_enabled = true;

        if (waitTime == -1f)
        {
            cinemachinePOV.m_HorizontalRecentering.m_WaitTime = DefaultHorizontalWaitTime;
        }

        if (recenteringTime == -1f)
        {
            cinemachinePOV.m_HorizontalRecentering.m_RecenteringTime = DefaultHorizontalRecenteringTime;
        }

        cinemachinePOV.m_HorizontalRecentering.m_WaitTime = DefaultHorizontalWaitTime;
        cinemachinePOV.m_HorizontalRecentering.m_RecenteringTime = DefaultHorizontalRecenteringTime;
    }

    public void DisableRecentering()
    {
        cinemachinePOV.m_HorizontalRecentering.m_enabled = false;
    }

    public void Init()
    {
        cinemachinePOV = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
    }
}