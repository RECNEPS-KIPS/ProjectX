// author:KIPKIPS
// describe:通用工具类
using System;
using Framework.Core.Manager.Config;
using Framework.Core.Manager.Timer;
using UnityEngine;

namespace Framework.Common {
    public static class CommonUtils {
        // 查找对象
        public static T Find<T>(Transform root, string name) {
            if (name == null) {
                T res = root.GetComponent<T>();
                return res;
            } else {
                Transform target = GetChild(root, name);
                if (target != null) {
                    T res = target.GetComponent<T>();
                    return res;
                } else {
                    return default;
                }
            }
        }
        public static T Find<T>(string name) {
            Transform target = GameObject.Find(name).transform;
            T res = target.GetComponent<T>();
            return res;
        }

        // 递归查找父节点下的对象
        private static Transform GetChild(Transform root, string childName) {
            //根节点查找
            Transform childTrs = root.Find(childName);
            if (childTrs != null) {
                return childTrs;
            }
            //遍历子物体查找
            int count = root.childCount;
            for (int i = 0; i < count; i++) {
                childTrs = GetChild(root.GetChild(i), childName);
                if (childTrs != null) {
                    return childTrs;
                }
            }
            return null;
        }
        public static string AddColor(string str, int colorIndex) {
            //其中ColorHelper.GetColor(color) 返回十六进制格式的颜色对象,color参数传入色码号即可,色码号在配置表可以看到
            //返回一个类似html标签语言包装好颜色信息的字符串,Unity的Text组件可以解析此串中的颜色信息
            return string.Format("<color={0}>{1}</color>", GetColor(colorIndex), str);
        }
        public static string GetColor(int colorIndex) {
            return ConfigManager.Instance.GetConfig("color")[colorIndex]["hexCode"].ToString();
        }

        //color下划线颜色 line 线厚度
        // public static string AddUnderLine(string msg, int colorIndex, int line) {
        //     return string.Format("<UnderWave/color={0},thickness=${1}>{2}</UnderWave>", GetColor(colorIndex), line, msg);
        // }

        // 获取随机颜色 默认alpha=1 
        public static Color GetRandomColor(bool isAlphaRandom = false) {
            Vector3 tempColorVec = new Vector3(new System.Random().Next(255), new System.Random().Next(255), new System.Random().Next(255)) / 255;
            return new Color(tempColorVec.x, tempColorVec.y, tempColorVec.z, isAlphaRandom ? new System.Random().Next() / 255 : 1);
        }

        // 获取随机颜色RGB A 例如：#ff00ff
        private static System.Random m_Random = new System.Random();
        public static String GetRandomColorCode() {
            int r = m_Random.Next(255);
            int g = m_Random.Next(255);
            int b = m_Random.Next(255);
            return String.Format("#{0}{1}{2}", r.ToString("X"), g.ToString("X"), b.ToString("X"));
        }

        // 单次定时器
        public static int SetTimeout(int ms, Action<TimerSlice> callback) {
            return TimerManager.Instance.SetTimeout(ms, callback);
        }

        // 循环定时器
        public static int SetInterval(int ms, Action<TimerSlice> callback) {
            return TimerManager.Instance.SetInterval(ms, callback);
        }
        public static void ClearTimer(int id) {
            TimerManager.Instance.ClearTimer(id);
        }
    }
}