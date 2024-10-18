// author:KIPKIPS
// date:2022.05.13 01:37
// describe:相机管理器

using Framework.Core.Singleton;
using UnityEngine;

namespace Framework.Core.Manager.Camera
{
    /// <summary>
    /// 相机管理器
    /// </summary>
    [MonoSingletonPath("[Manager]/CameraManager")]
    public class CameraManager : MonoSingleton<CameraManager>
    {
        private Transform _uiCameraRoot;
        private Transform UICameraRoot => _uiCameraRoot ??= GameObject.Find("[UI Camera]").transform;
        private UnityEngine.Camera _uiCamera;

        /// <summary>
        /// UI相机
        /// </summary>
        public UnityEngine.Camera UICamera => _uiCamera ? _uiCamera : UICameraRoot.GetComponent<UnityEngine.Camera>();

        // private Transform _mainCameraRoot;
        // private Transform MainCameraRoot => _mainCameraRoot ??= GameObject.Find("[Main Camera]").transform;
        // private UnityEngine.Camera _mainCamera;

        // /// <summary>
        // /// 主相机
        // /// </summary>
        // public UnityEngine.Camera MainCamera => _mainCamera ??= MainCameraRoot.GetComponent<UnityEngine.Camera>();

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Initialize()
        {
            DontDestroyOnLoad(UICameraRoot);
        }
    }
}