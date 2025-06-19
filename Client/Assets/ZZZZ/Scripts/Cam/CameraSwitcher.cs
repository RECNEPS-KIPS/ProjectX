using Cinemachine;
using HuHu;
using System.Collections.Generic;
using ZZZ;
using UnityEngine;

public class CameraSwitcher : Singleton<CameraSwitcher>
{
    private CinemachineBrain brain;

    // [SerializeField, Header("SwitchCharacterSkillCamera")] private CinemachineVirtualCamera switchCharacterSkillCamera;
    [System.Serializable]
    public class CharacterStateCameraInfo
    {
        public CharacterNameList characterName;
        public List<StateCameraInfo> stateCameraList = new List<StateCameraInfo>();
    }

    [System.Serializable]
    public class StateCameraInfo
    {
        public AttackStyle AttackStyle;
        public CinemachineStateDrivenCamera stateCamera;
    }

    [SerializeField, Header("StateCameraInfoList")]
    private List<CharacterStateCameraInfo> stateCameraInfoList = new List<CharacterStateCameraInfo>();

    private Dictionary<CharacterNameList, Dictionary<AttackStyle, CinemachineStateDrivenCamera>> stateCameraPool =
        new Dictionary<CharacterNameList, Dictionary<AttackStyle, CinemachineStateDrivenCamera>>();


    protected override void Awake()
    {
        base.Awake();
        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    private void Start()
    {
        InitSwitchCamera();
        // InitSkillCamera();
    }

    private void InitSwitchCamera()
    {
        if (stateCameraInfoList.Count == 0)
        {
            return;
        }

        for (int i = 0; i < stateCameraInfoList.Count; i++)
        {
            if (stateCameraInfoList[i].stateCameraList.Count == 0)
            {
                continue;
            }

            stateCameraPool.Add(stateCameraInfoList[i].characterName,
                new Dictionary<AttackStyle, CinemachineStateDrivenCamera>());
            for (int j = 0; j < stateCameraInfoList[i].stateCameraList.Count; j++)
            {
                stateCameraInfoList[i].stateCameraList[j].stateCamera.Priority = 0;

                stateCameraPool[stateCameraInfoList[i].characterName].Add(
                    stateCameraInfoList[i].stateCameraList[j].AttackStyle,
                    stateCameraInfoList[i].stateCameraList[j].stateCamera);
            }
        }
    }

    // private void InitSkillCamera()
    // {
    //     switchCharacterSkillCamera.Priority = 0;
    // }
    public void ActiveStateCamera(CharacterNameList characterName, AttackStyle attackStyle)
    {
        if (stateCameraPool.TryGetValue(characterName, out var stateCameraList))
        {
            if (stateCameraList.TryGetValue(attackStyle, out var stateDrivenCamera))
            {
                stateDrivenCamera.Priority = 20;
            }
        }
    }

    public void UnActiveStateCamera(CharacterNameList characterName, AttackStyle attackStyle)
    {
        if (stateCameraPool.TryGetValue(characterName, out var stateCameraList))
        {
            if (stateCameraList.TryGetValue(attackStyle, out var stateDrivenCamera))
            {
                stateDrivenCamera.Priority = 0;
            }
        }
    }

    public void ActiveSwitchCamera(bool applySwitchCamera)
    {
        // if (applySwitchCamera)
        // {
        //     switchCharacterSkillCamera.Priority = 20;
        // }
        // else
        // {
        //     switchCharacterSkillCamera.Priority = 0;
        // }
    }

    private void OnEnable()
    {
        brain.m_CameraActivatedEvent.AddListener(OnCameraActivated);
    }

    private void OnDisable()
    {
        brain.m_CameraActivatedEvent.RemoveListener(OnCameraActivated);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="newCamera"></param>
    /// <param name="oldCamera"></param>
    private void OnCameraActivated(ICinemachineCamera newCamera, ICinemachineCamera oldCamera)
    {
    }
}