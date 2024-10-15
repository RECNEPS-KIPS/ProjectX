// author:KIPKIPS
// describe:管理UI框架
using System.Collections.Generic;
using UnityEngine;
using Framework.Core.Singleton;
using UnityEditor;
using Framework.Core.Manager.Camera;
using Framework.Core.Manager.ResourcesLoad;
namespace Framework.Core.Manager.UI {
    [MonoSingletonPath("[Manager]/UIManager")]
    public class UIManager : MonoSingleton<UIManager> {
        private string logTag = "UIManager";
        private Stack<BaseWindow> _windowStack = new Stack<BaseWindow>();
        private Dictionary<int, BaseWindow> _baseWindowDict = new Dictionary<int, BaseWindow>();
        private Dictionary<int, WindowData> _windowDict = new Dictionary<int, WindowData>();
        private Transform UICameraRoot {
            get => CameraManager.Instance.UICamera.transform;
        }
        private UnityEngine.Camera _uiCamera;
        private UnityEngine.Camera UICamera {
            get => CameraManager.Instance.UICamera;
        }
        public override void Initialize() {
            AnalysisWindow();
            InitUICamera();
        }
        private void InitUICamera() {
            DontDestroyOnLoad(UICameraRoot);
        }
        public void Launch() {
            Open(UIWindow.InitializeWindow);
        }
        public void Open(UIWindow id, dynamic options = null) {
            WindowStackPush((int)id, options);
        }
        private BaseWindow GetWindowById(int id) {
            BaseWindow window;
            _baseWindowDict.TryGetValue(id, out window);
            if (window != null) {
                return window;
            } else {
                string path = _windowDict[id].path;
                GameObject windowObj = null;
#if UNITY_EDITOR
                windowObj = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ResourcesAssets/" + _windowDict[id].path + ".prefab"), UICameraRoot);
#else
                windowObj = Instantiate(ResourcesLoadManager.Instance.LoadFromFile<GameObject>(_windowDict[id].AssetTag,_windowDict[id].name),UICameraRoot);
#endif
                windowObj.name = _windowDict[id].name;
                window = windowObj.transform.GetComponent<BaseWindow>();
                window.WindowId = (UIWindow)id;
                _baseWindowDict.Add(id, window);
                return window;
            }
        }
        private void WindowStackPush(int windowId, dynamic options = null) {
            BaseWindow window = GetWindowById(windowId);
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
        public void Close(UIWindow id) {
            BaseWindow window = GetWindowById((int)id);
            LogManager.Log("Close Window === ", window.name);
            window.OnExit();
        }
        public void WindowStackPop() {
            if (_windowStack.Count > 0) {
                _windowStack.Pop(); //关闭栈顶界面
                // Destroy(window.gameObject);
                if (_windowStack.Count > 0) {
                    _windowStack.Peek().OnResume(); //恢复原先的界面
                }
            }
        }
        private void AnalysisWindow() {
            WindowMap windowMap = null;
#if UNITY_EDITOR
            windowMap = AssetDatabase.LoadAssetAtPath<WindowMap>("Assets/ResourcesAssets/RuntimeDepend/UIFramework/WindowMap.asset");
#else
            windowMap = ResourcesLoadManager.Instance.LoadFromFile<WindowMap>("RuntimeDepend","WindowMap".ToLower());
#endif
            foreach (WindowData windowData in windowMap.windowDataList) {
                windowData.AssetTag = windowData.path.Replace("/" + windowData.name, "");
                _windowDict.Add((int)windowData.id, windowData);
            }
            LogManager.Log(logTag, "Window data is parsed");
        }
    }
}