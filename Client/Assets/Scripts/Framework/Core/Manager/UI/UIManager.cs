// author:KIPKIPS
// describe:管理UI框架
using System.Collections.Generic;
using UnityEngine;
using Framework.Core.Singleton;
using UnityEditor;
using Framework.Core.Manager.Camera;
using Framework.Core.Manager.ResourcesLoad;
namespace Framework.Core.Manager.UI {
    /// <summary>
    /// UI框架管理器
    /// </summary>
    [MonoSingletonPath("[Manager]/UIManager")]
    public class UIManager : MonoSingleton<UIManager> {
        private const string LOGTag = "UIManager";
        private readonly Stack<BaseWindow> _windowStack = new Stack<BaseWindow>();
        private readonly Dictionary<int, BaseWindow> _baseWindowDict = new Dictionary<int, BaseWindow>();
        private readonly Dictionary<int, WindowData> _windowDict = new Dictionary<int, WindowData>();
        private static Transform UICameraRoot => CameraManager.Instance.UICamera.transform;
        private UnityEngine.Camera _uiCamera;
        private static UnityEngine.Camera UICamera => CameraManager.Instance.UICamera;

        /// <summary>
        /// UI框架初始化
        /// </summary>
        public override void Initialize() {
            AnalysisWindow();
            InitUICamera();
        }
        private void InitUICamera() {
            DontDestroyOnLoad(UICameraRoot);
        }
        /// <summary>
        /// UI框架启动
        /// </summary>
        public void Launch() {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="options"></param>
        public void Open(UIWindow id, dynamic options = null) {
            WindowStackPush((int)id, options);
        }
        private BaseWindow GetWindowById(int id) {
            _baseWindowDict.TryGetValue(id, out var window);
            if (window != null) {
                return window;
            }

            var path = _windowDict[id].path;
#if UNITY_EDITOR
            var windowObj = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ResourcesAssets/" + _windowDict[id].path + ".prefab"), UICameraRoot);
#else
            var GameObject windowObj = Instantiate(ResourcesLoadManager.Instance.LoadFromFile<GameObject>(_windowDict[id].AssetTag,_windowDict[id].name),UICameraRoot);
#endif
            windowObj.name = _windowDict[id].name;
            window = windowObj.transform.GetComponent<BaseWindow>();
            window.WindowId = (UIWindow)id;
            _baseWindowDict.Add(id, window);
            return window;
        }
        private void WindowStackPush(int windowId, dynamic options = null) {
            var window = GetWindowById(windowId);
            LogManager.Log("Open Window === ", window.name);
            window.Canvas.sortingOrder = _windowDict[windowId].layer;
            window.Canvas.worldCamera = UICamera;
            if (window.IsShow) {
                return;
            }

            //显示当前界面时,应该先去判断当前栈是否为空,不为空说明当前有界面显示,把当前的界面OnPause掉
            if (_windowStack.Count > 0) {
                _windowStack.Peek().OnPause();
            }

            //每次入栈(显示页面的时候),触发window的OnEnter方法
            window.OnEnter(options);
            _windowStack.Push(window);
        }
        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="id"></param>
        public void Close(UIWindow id) {
            var window = GetWindowById((int)id);
            LogManager.Log("Close Window === ", window.name);
            window.OnExit();
        }
        /// <summary>
        /// 
        /// </summary>
        public void WindowStackPop()
        {
            if (_windowStack.Count <= 0) return;
            _windowStack.Pop(); //关闭栈顶界面
            // Destroy(window.gameObject);
            if (_windowStack.Count > 0) {
                _windowStack.Peek().OnResume(); //恢复原先的界面
            }
        }
        private void AnalysisWindow() {
#if UNITY_EDITOR
            var windowMap = AssetDatabase.LoadAssetAtPath<WindowMap>("Assets/ResourcesAssets/RuntimeDepend/UIFramework/WindowMap.asset");
#else
            var windowMap = ResourcesLoadManager.Instance.LoadFromFile<WindowMap>("RuntimeDepend","WindowMap".ToLower());
#endif
            if (windowMap is null || windowMap.windowDataList is null)
            {
                return;
            }
            foreach (var windowData in windowMap.windowDataList) {
                windowData.AssetTag = windowData.path.Replace("/" + windowData.name, "");
                _windowDict.Add((int)windowData.id, windowData);
            }
            LogManager.Log(LOGTag, "Window data is parsed");
        }
    }
}