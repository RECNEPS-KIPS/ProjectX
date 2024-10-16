// author:KIPKIPS
// describe:UI Window数据
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core.Manager.UI {
    /// <summary>
    /// 界面数据的列表类
    /// </summary>
    [CreateAssetMenu(fileName = "WindowMap", menuName = "Framework/WindowMap")]
    public class WindowMap : ScriptableObject {
        /// <summary>
        /// 所有界面数据的列表
        /// </summary>
        public List<WindowData> windowDataList;
    }
}