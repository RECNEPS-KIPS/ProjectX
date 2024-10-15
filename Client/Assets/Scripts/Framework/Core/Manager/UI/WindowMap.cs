// author:KIPKIPS
// describe:UI Window数据
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core.Manager.UI {
    [CreateAssetMenu(fileName = "WindowMap", menuName = "Framework/WindowMap")]
    public class WindowMap : ScriptableObject {
        public List<WindowData> windowDataList;
    }
}