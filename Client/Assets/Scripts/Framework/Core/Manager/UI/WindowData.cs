// author:KIPKIPS
// describe:面板数据
using System;

namespace Framework.Core.Manager.UI {
    [Serializable]
    public class WindowData {
        /// <summary>
        /// 界面ID
        /// </summary>
        public UIWindow id;
        /// <summary>
        /// 界面名称
        /// </summary>
        public string name;
        /// <summary>
        /// 界面资源路径
        /// </summary>
        public string path;
        /// <summary>
        /// 所属层级
        /// </summary>
        public int layer;
        /// <summary>
        /// 界面类型
        /// </summary>
        public WindowType windowType;
        /// <summary>
        /// 
        /// </summary>
        public string AssetTag { get; set; }
        /// <summary>
        /// 构造界面基础数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="layer"></param>
        /// <param name="windowType"></param>
        public WindowData(UIWindow id, string name, string path, int layer, WindowType windowType) {
            this.id = id;
            this.name = name;
            this.path = path;
            this.layer = layer;
            this.windowType = windowType;
        }
    }
    /// <summary>
    /// 窗口类型
    /// </summary>
    public enum WindowType {
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
}