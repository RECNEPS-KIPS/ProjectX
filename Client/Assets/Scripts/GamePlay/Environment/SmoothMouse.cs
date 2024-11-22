// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
// using Framework.Core.Manager.Input;
//
// namespace GamePlay.Environment
// {
//     public class SmoothMouse : MonoBehaviour
//     {
//         public enum RotationAxes
//         {
//             MouseXAndY = 0,
//             MouseX = 1,
//             MouseY = 2
//         }
//
//         public RotationAxes axes = RotationAxes.MouseXAndY;
//         public float sensitivityX = 15f;
//         public float sensitivityY = 15f;
//
//         public float minimumX = -360f;
//         public float maximumX = 360f;
//
//         public float minimumY = -60f;
//         public float maximumY = 60f;
//
//         private float rotationX;
//         private float rotationY;
//
//         private readonly List<float> rotArrayX = new();
//         private float rotAverageX;
//
//         private readonly List<float> rotArrayY = new();
//         private float rotAverageY;
//
//         public float frameCounter = 20;
//
//         public bool invertY = true;
//         private Quaternion originalRotation;
//         private bool _lockCursor = true;
//         public Texture2D crossHairImage;
//         private void OnGUI()
//         {
//             if (!_lockCursor) return;
//             const float size = 0.25f;
//             float xMin = Screen.width / 2f - crossHairImage.width * size / 2;
//             float yMin = Screen.height / 2f - crossHairImage.height * size / 2;
//             GUI.DrawTexture(new Rect(xMin, yMin, crossHairImage.width * size, crossHairImage.height * size), crossHairImage);
//         }
//
//         void Update()
//         {
//             if (_lockCursor)
//             {
//                 HandleCamera();
//             }
//             HandleCursor();
//         }
//
//         private void HandleCamera()
//         {
//             if (axes == RotationAxes.MouseXAndY)
//             {
//                 rotAverageY = 0f;
//                 rotAverageX = 0f;
//                 
//                 rotationY += InputManager.Instance.GetAxisInput().y * sensitivityY;
//                 rotationX += InputManager.Instance.GetAxisInput().x * sensitivityX;
//
//                 rotArrayY.Add(rotationY);
//                 rotArrayX.Add(rotationX);
//
//                 if (rotArrayY.Count >= frameCounter)
//                 {
//                     rotArrayY.RemoveAt(0);
//                 }
//
//                 if (rotArrayX.Count >= frameCounter)
//                 {
//                     rotArrayX.RemoveAt(0);
//                 }
//
//                 foreach (var t in rotArrayY)
//                 {
//                     rotAverageY += t;
//                 }
//
//                 foreach (var t in rotArrayX)
//                 {
//                     rotAverageX += t;
//                 }
//
//                 rotAverageY /= rotArrayY.Count;
//                 rotAverageX /= rotArrayX.Count;
//
//                 rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);
//                 rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);
//
//                 Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
//                 Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);
//
//                 transform.localRotation = originalRotation * xQuaternion * yQuaternion;
//             }
//             else if (axes == RotationAxes.MouseX)
//             {
//                 rotAverageX = 0f;
//
//                 rotationX += InputManager.Instance.GetAxisInput().x * sensitivityX;
//
//                 rotArrayX.Add(rotationX);
//
//                 if (rotArrayX.Count >= frameCounter)
//                 {
//                     rotArrayX.RemoveAt(0);
//                 }
//
//                 foreach (var t in rotArrayX)
//                 {
//                     rotAverageX += t;
//                 }
//
//                 rotAverageX /= rotArrayX.Count;
//
//                 rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);
//
//                 Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);
//                 transform.localRotation = originalRotation * xQuaternion;
//             }
//             else
//             {
//                 rotAverageY = 0f;
//
//                 rotationY += InputManager.Instance.GetAxisInput().y * sensitivityY * (invertY ? 1 : -1);
//
//                 rotArrayY.Add(rotationY);
//
//                 if (rotArrayY.Count >= frameCounter)
//                 {
//                     rotArrayY.RemoveAt(0);
//                 }
//
//                 foreach (var t in rotArrayY)
//                 {
//                     rotAverageY += t;
//                 }
//
//                 rotAverageY /= rotArrayY.Count;
//
//                 rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);
//
//                 Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
//                 transform.localRotation = originalRotation * yQuaternion;
//             }
//         }
//
//         private void HandleCursor()
//         {
//             Cursor.visible = !_lockCursor;
//             Cursor.lockState = _lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
//             if (Input.GetKeyDown(KeyCode.Escape)) {
//                 
//                 _lockCursor = !_lockCursor;
//             }
//         }
//
//         private void HandleMovement()
//         {
//             var speed = 4f;
//             if (Mathf.Abs(InputManager.Instance.GetMoveInput().y) > 0.01f)
//             {
//                 var transform1 = transform;
//                 transform1.position += transform1.forward * Time.deltaTime * speed;
//             }
//
//             if (Mathf.Abs(InputManager.Instance.GetMoveInput().x) > 0.01f)
//             {
//                 var transform1 = transform;
//                 transform1.position -= transform1.right * Time.deltaTime * speed;
//             }
//
//             if (Mathf.Abs(InputManager.Instance.GetMoveInput().y) > 0.01f)
//             {
//                 var transform1 = transform;
//                 transform1.position -= transform1.forward * Time.deltaTime * speed;
//             }
//
//             if (Mathf.Abs(InputManager.Instance.GetMoveInput().x) > 0.01f)
//             {
//                 var transform1 = transform;
//                 transform1.position += transform1.right * Time.deltaTime * speed;
//             }
//         }
//
//         void Start()
//         {
//             Rigidbody rb = GetComponent<Rigidbody>();
//             if (rb)
//             {
//                 rb.freezeRotation = true;
//             }
//             originalRotation = transform.localRotation;
//         }
//
//         public static float ClampAngle(float angle, float min, float max)
//         {
//             angle %= 360;
//             if (angle is >= -360f and <= 360f)
//             {
//                 if (angle < -360f)
//                 {
//                     angle += 360f;
//                 }
//
//                 if (angle > 360f)
//                 {
//                     angle -= 360f;
//                 }
//             }
//
//             return Mathf.Clamp(angle, min, max);
//         }
//     }
// }