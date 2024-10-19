// author:KIPKIPS
// describe:管理UI框架

using System;
using System.Collections.Generic;
using Framework.Core.Container;
using UnityEngine;
using Framework.Core.Singleton;
using UnityEditor;
using Framework.Core.Manager.Camera;
using Framework.Core.Manager.ResourcesLoad;
using Framework.Core.ResourcesAssets;

namespace Framework.Core.Manager.UI
{
    /// <summary>
    /// 窗口类型
    /// </summary>
    public enum WindowType
    {
        /// <summary>
        /// 自由弹窗
        /// </summary>
        Freedom = 1,

        /// <summary>
        /// 固定弹窗
        /// </summary>
        Fixed = 2,

        /// <summary>
        /// 栈类型弹窗
        /// </summary>
        Stack = 3,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum WindowNameDef
    {
        ExampleUI = 0,
        StartWindow = 1,
        PlotWindow = 2,
    }

    /// <summary>
    /// UI框架管理器
    /// </summary>
    [MonoSingletonPath("[Manager]/UIManager")]
    public class UIManager : MonoSingleton<UIManager>
    {
        /// <summary>
        /// Window数据类
        /// </summary>
        [Serializable]
        public class WindowData
        {
            /// <summary>
            /// 界面名称
            /// </summary>
            public string Name;

            /// <summary>
            /// 界面ID
            /// </summary>
            public WindowNameDef ID;

            /// <summary>
            /// 界面资源路径
            /// </summary>
            public string UIPrefabPath;

            /// <summary>
            /// 所属层级
            /// </summary>
            public int Layer;

            /// <summary>
            /// 界面类型
            /// </summary>
            public WindowType WindowType;
        }

        /// <summary>
        /// 定义各个UI界面数据
        /// </summary>
        private readonly Dictionary<WindowNameDef, WindowData> WindowDataDict = new()
        {
            {
                WindowNameDef.ExampleUI,
                new WindowData
                {
                    UIPrefabPath = "UI/Example/ExampleUI",
                    WindowType = WindowType.Stack
                }
            },
            {
                WindowNameDef.StartWindow,
                new WindowData
                {
                    UIPrefabPath = "UI/Start/StartWindow",
                    WindowType = WindowType.Stack
                }
            },
            {
                WindowNameDef.PlotWindow,
                new WindowData
                {
                    UIPrefabPath = "UI/Plot/PlotWindow",
                    WindowType = WindowType.Stack
                }
                
            }
        };

        private const string LOGTag = "UIManager";
        private readonly Stack<BaseWindow> _windowStack = new Stack<BaseWindow>();
        private readonly Dictionary<int, BaseWindow> _baseWindowDict = new Dictionary<int, BaseWindow>();

        private static Transform UICameraRoot => CameraManager.Instance.UICamera.transform;
        private UnityEngine.Camera _uiCamera;
        private static UnityEngine.Camera UICamera => CameraManager.Instance.UICamera;

        public Transform CanvasRoot
        {
            get
            {
                if (_canvasRoot != null) return _canvasRoot;
                _canvasRoot = UICameraRoot.Find("UIRoot");
                return _canvasRoot;
            }
        }

        private Transform _canvasRoot;

        /// <summary>
        /// UI框架初始化
        /// </summary>
        public override void Initialize()
        {
            RegistUIBinding();
            InitWindowData();
        }

        void RegistUIBinding()
        {
            UIBinding.Register();
        }

        private void InitUICamera()
        {
            DontDestroyOnLoad(UICameraRoot);
        }

        /// <summary>
        /// UI框架启动
        /// </summary>
        public void Launch()
        {
            InitUICamera();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="options"></param>
        public void Open(WindowNameDef id, dynamic options = null)
        {
            WindowStackPush(id, options);
        }

        private BaseWindow GetWindowById(WindowNameDef id,bool IsAsync = false)
        {
            _baseWindowDict.TryGetValue((int)id, out var window);
            if (window != null)
            {
                return window;
            }
            
            var go = Instantiate(ResourcesLoadManager.LoadAsset<GameObject>($"{DEF.RESOURCES_ASSETS_PATH}/{WindowDataDict[id].UIPrefabPath}.prefab"),CanvasRoot);
            window = go.transform.GetComponent<BaseWindow>();
            go.transform.name = WindowDataDict[id].Name;
            window.WindowId = id;
            _baseWindowDict.Add((int)id, window);
            window.OnInit();
            return window;
        }

        private void WindowStackPush(WindowNameDef windowId, dynamic options = null)
        {
            var window = GetWindowById(windowId);
            LogManager.Log("Open Window === ", window.name);
            // window.Canvas.sortingOrder = 0;//WindowDataDict[windowId].Layer;
            // window.Canvas.worldCamera = UICamera;
            if (window.IsShow)
            {
                return;
            }

            //显示当前界面时,应该先去判断当前栈是否为空,不为空说明当前有界面显示,把当前的界面OnPause掉
            if (_windowStack.Count > 0)
            {
                _windowStack.Peek().OnPause();
            }

            //每次入栈(显示页面的时候),触发window的OnEnter方法
            window.OnEnter(options);
            window.transform.gameObject.SetActive(true);
            _windowStack.Push(window);
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="id"></param>
        public void Close(WindowNameDef id)
        {
            var window = GetWindowById(id);
            window.OnExit();
            window.transform.gameObject.SetActive(false);
            LogManager.Log("Close Window === ", window.name);
        }

        /// <summary>
        /// 
        /// </summary>
        public void WindowStackPop()
        {
            if (_windowStack.Count <= 0) return;
            _windowStack.Pop(); //关闭栈顶界面
            // Destroy(window.gameObject);
            if (_windowStack.Count > 0)
            {
                _windowStack.Peek().OnResume(); //恢复原先的界面
            }
        }

        private void InitWindowData()
        {
            foreach (var kvp in WindowDataDict)
            {
                kvp.Value.ID = kvp.Key;
                kvp.Value.Name = kvp.Key.ToString();
            }

            LogManager.Log(LOGTag, "Window data is parsed");
        }
    }
}