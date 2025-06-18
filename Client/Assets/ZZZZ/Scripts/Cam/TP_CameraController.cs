using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TP_CameraController : MonoBehaviour
{  
    //获取鼠标的输入
    //应用旋转
    //应用位置
    [SerializeField, Header("目标对象")] private Transform camLookTarget;
    [SerializeField, Header("距离对象的距离")] private float camToTargetDistance;
    [SerializeField, Header("X&Y灵敏度")] private Vector2 mouseInputSpeed;   
    [Range(0.02f, 0.5f), SerializeField, Header("旋转平滑时间")] private float camSmoothTime;    
    [SerializeField, Header("镜头移动跟随速度")] private float camLerpSpeedOnMouce;
    [SerializeField, Header("镜头未移动跟随速度")] private float camLerpSpeedOnNormal;
    [SerializeField] private float currentCamLerpSpeed;
    [SerializeField, Header("垂直限制角")] private Vector2 camClampRange;
    private Transform Cam;
    private float Yaw;
    private float Pitch;
    private Vector3 camEulerAngles;
    private Vector3 camRotationPos;
    private Vector3 rotaionCurrentVelocity=Vector3.zero;
  
    private void Awake()
    {
       Cam=Camera.main.transform;
        
    }
    private void Start()
    {
        currentCamLerpSpeed = camLerpSpeedOnNormal;
    }
    private void Update()
    {
        GetMouceInput();
        UpdateCursor();
       // SetCamLerpSpeed();
    }
    private void LateUpdate()
    {
       CameraController();
    }
    private void GetMouceInput()
    {
        Yaw += CharacterInputSystem.MainInstance.CameraLook.x * mouseInputSpeed.x;
        Pitch -= CharacterInputSystem.MainInstance.CameraLook.y * mouseInputSpeed.y;
        Pitch = Mathf.Clamp(Pitch, camClampRange.x, camClampRange.y);
    }
    
    private void CameraController()
    {
        //相机的转向
         camEulerAngles = Vector3.SmoothDamp(camEulerAngles, new Vector3(Pitch, Yaw), ref rotaionCurrentVelocity, camSmoothTime);
         transform.eulerAngles = camEulerAngles;
        //相机的位置
        camRotationPos = camLookTarget.position - transform.forward * camToTargetDistance;
        transform.position = Vector3.Lerp(transform.position, camRotationPos, currentCamLerpSpeed * Time.deltaTime);
      
    }
    private void SetCamLerpSpeed()
    {
        if (Mathf.Abs( CharacterInputSystem.MainInstance.CameraLook.x) > 0.9f)
        {
            currentCamLerpSpeed = Mathf.Lerp(currentCamLerpSpeed, camLerpSpeedOnMouce, Time.deltaTime * 10);
        }
        else
        {
            currentCamLerpSpeed = Mathf.Lerp(currentCamLerpSpeed, camLerpSpeedOnNormal, Time.deltaTime * 10);
        }
    }
        private void UpdateCursor()
    { 
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
   
}
