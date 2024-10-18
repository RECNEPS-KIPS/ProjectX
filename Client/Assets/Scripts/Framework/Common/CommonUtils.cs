// author:KIPKIPS
// describe:通用工具类

using System;
using Framework.Core.Manager.Config;
using Framework.Core.Manager.Timer;
using UnityEngine;

namespace Framework.Common
{
    /// <summary>
    /// 通用工具
    /// </summary>
    public static class CommonUtils
    {
        /// <summary>
        /// 查找对象
        /// </summary>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Find<T>(Transform root, string name)
        {
            if (name == null)
            {
                var res = root.GetComponent<T>();
                return res;
            }
            else
            {
                var target = GetChild(root, name);
                if (target == null) return default;
                var res = target.GetComponent<T>();
                return res;
            }
        }

        /// <summary>
        /// 查找对象
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Find<T>(string name)
        {
            var target = GameObject.Find(name).transform;
            var res = target.GetComponent<T>();
            return res;
        }

        // 递归查找父节点下的对象
        private static Transform GetChild(Transform root, string childName)
        {
            //根节点查找
            var childTrs = root.Find(childName);
            if (childTrs != null)
            {
                return childTrs;
            }

            //遍历子物体查找
            var count = root.childCount;
            for (var i = 0; i < count; i++)
            {
                childTrs = GetChild(root.GetChild(i), childName);
                if (childTrs != null)
                {
                    return childTrs;
                }
            }

            return null;
        }

        /// <summary>
        /// 添加颜色
        /// </summary>
        /// <param name="str"></param>
        /// <param name="colorIndex"></param>
        /// <returns></returns>
        public static string AddColor(string str, int colorIndex)
        {
            //其中ColorHelper.GetColor(color) 返回十六进制格式的颜色对象,color参数传入色码号即可,色码号在配置表可以看到
            //返回一个类似html标签语言包装好颜色信息的字符串,Unity的Text组件可以解析此串中的颜色信息
            return $"<color={GetColor(colorIndex)}>{str}</color>";
        }

        /// <summary>
        /// 获取颜色
        /// </summary>
        /// <param name="colorIndex"></param>
        /// <returns></returns>
        public static string GetColor(int colorIndex)
        {
            return ConfigManager.GetConfig(ConfigNameDef.Color)[colorIndex]["hexCode"].ToString();
        }

        //color下划线颜色 line 线厚度
        // public static string AddUnderLine(string msg, int colorIndex, int line) {
        //     return string.Format("<UnderWave/color={0},thickness=${1}>{2}</UnderWave>", GetColor(colorIndex), line, msg);
        // }

        // 
        /// <summary>
        /// 获取随机颜色 默认alpha=1 
        /// </summary>
        /// <param name="isAlphaRandom"></param>
        /// <returns></returns>
        public static Color GetRandomColor(bool isAlphaRandom = false)
        {
            var tempColorVec = new Vector3(new System.Random().Next(255), new System.Random().Next(255),
                new System.Random().Next(255)) / 255;
            return new Color(tempColorVec.x, tempColorVec.y, tempColorVec.z,
                isAlphaRandom ? (float)new System.Random().Next() / 255 : 1);
        }

        private static readonly System.Random Random = new();

        /// <summary>
        /// 获取随机颜色RGB A 例如：#ff00ff
        /// </summary>
        /// <returns></returns>
        public static string GetRandomColorCode()
        {
            return $"#{Random.Next(255):X}{Random.Next(255):X}{Random.Next(255):X}";
        }

        /// <summary>
        /// 单次定时器
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static int SetTimeout(int ms, Action<TimerSlice> callback)
        {
            return TimerManager.Instance.SetTimeout(ms, callback);
        }

        /// <summary>
        /// 循环定时器
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static int SetInterval(int ms, Action<TimerSlice> callback)
        {
            return TimerManager.Instance.SetInterval(ms, callback);
        }

        /// <summary>
        /// 清理定时器
        /// </summary>
        /// <param name="id"></param>
        public static void ClearTimer(int id)
        {
            TimerManager.Instance.ClearTimer(id);
        }
    }
}