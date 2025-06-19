
using UnityEngine;
namespace HuHu
{
    public class CameraController : MonoBehaviour
    {
        #region
        private Transform cam;
        #endregion

        #region 
        private float Y_Pivot;
        private float X_Pivot;

        #endregion

        private Vector3 currentEulerAngler;
        private Vector3 currentVelocity = Vector3.zero;
        private Vector3 targetPosition;
        [SerializeField] Transform lookAt;

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
            CameraPosition();
            CameraRotation();

        }
        /// <summary>
        /// </summary>
        private void CameraRotation()
        {
            currentEulerAngler = Vector3.SmoothDamp(currentEulerAngler, new Vector3(X_Pivot, Y_Pivot, 0), ref currentVelocity, rotationTime);
            cam.eulerAngles = currentEulerAngler;
            //cam.rotation=Quaternion.Euler(currentEulerAngler);
        }

        /// <summary>
        /// </summary>
        private void CameraPosition()
        {
            targetPosition = lookAt.transform.position - cam.forward * distance;
            cam.position = Vector3.Lerp(cam.position, targetPosition, followSpeed * Time.deltaTime);
        }

        /// <summary>
        /// </summary>
        private void UpdateCameraInput()
        {
            Y_Pivot += CharacterInputSystem.MainInstance.CameraLook.x* X_Sensitivity;
            X_Pivot -= CharacterInputSystem.MainInstance.CameraLook.y* Y_Sensitivity;
            X_Pivot = Mathf.Clamp(X_Pivot, angleRange.x, angleRange.y);
        }

    }
}
