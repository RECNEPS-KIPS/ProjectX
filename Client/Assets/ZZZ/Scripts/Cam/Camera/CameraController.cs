
using UnityEngine;
namespace HuHu
{
    public class CameraController : MonoBehaviour
    {
        #region 第一是获取到相机
        private Transform cam;
        #endregion

        #region 声明输入变量
        private float Y_Pivot;//鼠标的x方向输入
        private float X_Pivot;//鼠标的y方向输入

        #endregion

        private Vector3 currentEulerAngler;
        private Vector3 currentVelocity = Vector3.zero;
        private Vector3 targetPosition;
        [SerializeField] Transform lookAt;
        //控制参数
        [SerializeField] private Vector2 angleRange;
        [SerializeField] float distance;
        [SerializeField] private float rotationTime;
        [SerializeField] private float followSpeed;
        [SerializeField] private float X_Sensitivity;
        [SerializeField] private float Y_Sensitivity;
        private void Awake()
        {
            cam = Camera.main.transform;
        }
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        private void Update()
        {
            UpdateCameraInput();
        }
        
        private void LateUpdate()
        {
            //通常把改变物体旋转的比输入晚一帧执行
            CameraPosition();
            CameraRotation();

        }
        /// <summary>
        /// 旋转相机
        /// </summary>
        private void CameraRotation()
        {
            currentEulerAngler = Vector3.SmoothDamp(currentEulerAngler, new Vector3(X_Pivot, Y_Pivot, 0), ref currentVelocity, rotationTime);
            cam.eulerAngles = currentEulerAngler;
            //也可以用转成四元数
            //cam.rotation=Quaternion.Euler(currentEulerAngler);
        }

        /// <summary>
        /// 相机的位置
        /// </summary>
        private void CameraPosition()
        {
            //相机的目标位置
            targetPosition = lookAt.transform.position - cam.forward * distance;
            //lerp在不断调用*时间补偿的状态下，可以让一个值非线性地靠近目标值
            cam.position = Vector3.Lerp(cam.position, targetPosition, followSpeed * Time.deltaTime);
        }

        /// <summary>
        /// 鼠标的输入
        /// </summary>
        private void UpdateCameraInput()
        {
            //设置两个变量接受输入
            //这里我使用了新输入封装好的鼠标输入的属性
            Y_Pivot += CharacterInputSystem.MainInstance.CameraLook.x* X_Sensitivity;
            X_Pivot -= CharacterInputSystem.MainInstance.CameraLook.y* Y_Sensitivity;
            //用Clamp限制一下
            X_Pivot = Mathf.Clamp(X_Pivot, angleRange.x, angleRange.y);
        }

    }
}
