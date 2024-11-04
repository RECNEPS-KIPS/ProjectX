// author:KIPKIPS
// describe:通用工具类

using System;
using System.Collections;
using System.Reflection;
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
        
        public static Transform CreateNode(string nodeName, Transform parent = null)
        {
            var go = new GameObject(nodeName);
            var trs = go.transform;
            if (parent)
            {
                trs.SetParent(parent);
            }
            trs.localPosition = Vector3.zero;
            trs.localScale = Vector3.one;
            trs.localRotation = Quaternion.identity;
            return trs;
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
            return ConfigManager.GetConfig(EConfig.Color)[colorIndex]["hexCode"].ToString();
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

        public static string SetRichFontSize(string txt,int fontSize) {
            return $"<size={fontSize}>{txt}</size>";
        }
        
        public static string GetFormatNum(int num)
        {
            // var str = num.ToString();
            // var rst = string.Empty;
            // for (int i = str.Length - 1, l = 1; i >= 0; i--)
            // {
            //     rst = $"{str[i]}{rst}";
            //     if (l > 0 && i > 0 && l % 3 == 0)
            //     {
            //         rst = $",{rst}";
            //         l = 0;
            //     }
            //     l++;
            // }
            return $"{num:N0}";
            // return rst;
        }

        public static void ResetGO(Transform t,Transform parent = null)
        {
            if (parent)
            {
                t.SetParent(parent);
            }
            t.localPosition = Vector3.zero;
            t.localScale = Vector3.one;
            t.localRotation = Quaternion.identity;
        }

        public static void Clear<T>(this T[] array)
        {
            if (array == null)
                return;
            for (int i = 0, length = array.Length; i < length; i++)
            {
                array[i] = default;
            }
        }
        
        private static Collider[] m_checkColliders = new Collider[10];
        public static Collider[] OverlapSphere_Max10(Vector3 center,float radius,int layerValue)
        {
            m_checkColliders.Clear();
            Physics.OverlapSphereNonAlloc(center, radius,m_checkColliders,layerValue);
            return m_checkColliders;
        }
    }

    public class RefType<T> where T : class
    {
        private object m_instance;
        private Type m_type;

        public RefType(T instance)
        {
            m_instance = instance;
            m_type = instance.GetType();
        }

        public bool TryGetField<T>(string fieldName,out T result)
        {
            result = default(T);
            if (m_instance == null)
                return false;

            FieldInfo fieldInfo = m_type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (fieldInfo == null)
                return false;
            result = (T)fieldInfo.GetValue(m_instance);
            return true;
        }
    }
}