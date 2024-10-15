// author:KIPKIPS
// describe:面板数据
using System;

namespace Framework.Core.Manager.UI {
    [Serializable]
    public class WindowData {
        public UIWindow id;
        public string name;
        public string path;
        public int layer;
        public WindowType windowType;
        public string AssetTag { get; set; }
        public WindowData(UIWindow _id, string _name, string _path, int _layer, WindowType _windowType) {
            id = _id;
            name = _name;
            path = _path;
            layer = _layer;
            windowType = _windowType;
        }
    }
    public enum WindowType {
        Freedom = 1,
        Fixed = 2,
        Stack = 3,
    }
}