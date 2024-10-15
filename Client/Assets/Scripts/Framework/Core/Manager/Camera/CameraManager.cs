// author:KIPKIPS
// date:2022.05.13 01:37
// describe:相机管理器
using Framework.Core.Singleton;
using UnityEngine;

namespace Framework.Core.Manager.Camera {
    [MonoSingletonPath("[Manager]/CameraManager")]
    public class CameraManager : MonoSingleton<CameraManager> {
        private Transform _uiCameraRoot;
        private Transform UICameraRoot {
            get => _uiCameraRoot = _uiCameraRoot ?? GameObject.Find("[UI Camera]").transform;
        }
        private UnityEngine.Camera _uiCamera;
        public UnityEngine.Camera UICamera {
            get => _uiCamera = _uiCamera ?? UICameraRoot.GetComponent<UnityEngine.Camera>();
        }
        private Transform _mainCameraRoot;
        private Transform MainCameraRoot {
            get => _mainCameraRoot = _mainCameraRoot ?? GameObject.Find("[Main Camera]").transform;
        }
        private UnityEngine.Camera _mainCamera;
        public UnityEngine.Camera MainCamera {
            get => _mainCamera = _mainCamera ?? MainCameraRoot.GetComponent<UnityEngine.Camera>();
        }
        public override void Initialize() {
            DontDestroyOnLoad(UICameraRoot);
            DontDestroyOnLoad(MainCameraRoot);
        }
    }
}