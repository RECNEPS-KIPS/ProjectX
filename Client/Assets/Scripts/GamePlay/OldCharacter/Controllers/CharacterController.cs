// using Framework.Core.SpaceSegment;
// using UnityEngine;
//
// namespace GamePlay.Character
// {
//     public class CharacterController : MonoBehaviour
//     {
//         public Transform ModelMountRoot;
//         public Transform CameraMountRoot;
//         private Camera _camera;
//         public Camera Camera
//         {
//             get
//             {
//                 if (_camera != null) return _camera;
//                 if (CameraMountRoot != null)
//                 {
//                     _camera = CameraMountRoot.GetComponent<Camera>();
//                 } 
//                 else
//                 {
//                     LogManager.LogError("CharacterController","Not set CameraRoot");    
//                 }
//                 return _camera;
//             }
//         }
//         
//         private DetectorBase _sceneDetector;
//         public DetectorBase SceneDetector
//         {
//             get
//             {
//                 if (_sceneDetector != null) return _sceneDetector;
//                 if (CameraMountRoot != null)
//                 {
//                     _sceneDetector = CameraMountRoot.GetComponent<DetectorBase>();
//                 }
//                 else
//                 {
//                     LogManager.LogError("CharacterController","Not set CameraRoot");    
//                 }
//                 return _sceneDetector;
//             }
//         }
//     }
// }