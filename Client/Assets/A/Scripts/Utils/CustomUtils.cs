#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System;
using System.Collections.Generic;

namespace GameUtils
{
#if UNITY_EDITOR
    public static class CustonEditorUtils
    {
        private static Dictionary<string, Texture> m_editorIconCache = new Dictionary<string, Texture>();
        public static Texture GetEditorIcon(string iconPath)
        {
            if (m_editorIconCache.TryGetValue(iconPath, out var icon))
            {
                return icon;
            }
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{iconPath}");
            m_editorIconCache[iconPath] = icon;
            return icon;
        }
    }
#endif
}

namespace GameUtils
{
    public static class CustomUtils
    {
        public static Vector2Int ToVector2Int(this Vector3Int vector3Int)
        {
            return new Vector2Int(vector3Int.x, vector3Int.y);
        }

        public static Vector3Int ToVector3Int(this Vector2Int vector2Int)
        {
            return new Vector3Int(vector2Int.x, vector2Int.y, 0);
        }

        public static bool CheckIsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this List<TValue> list, Func<TValue, TKey> getKeyFunc)
        {
            Dictionary<TKey, TValue> resultDict = new Dictionary<TKey, TValue>();
            for (int i = 0, length = list.Count; i < length; i++)
            {
                var key = getKeyFunc(list[i]);
                if (resultDict.ContainsKey(key))
                {
                    Debug.LogError($"重复的key: {key}");
                }
                resultDict[key] = list[i];
            }
            return resultDict;
        }

        private static Queue<Transform> m_foreachChildTempQueue = new Queue<Transform>();
        public static void ForeachChildDoActionNoAlloc(Transform root, Action<Transform> doAction)
        {
            if (root == null)
            {
                return;
            }
            m_foreachChildTempQueue.Clear();
            m_foreachChildTempQueue.Enqueue(root);
            while (m_foreachChildTempQueue.Count > 0)
            {
                Transform child = m_foreachChildTempQueue.Dequeue();
                if (child != root)
                {
                    doAction(child);
                }
                foreach (Transform grandChild in child)
                {
                    m_foreachChildTempQueue.Enqueue(grandChild);
                }
            }

        }

        public static void SetActiveOptimize(this GameObject gameObject, bool active)
        {
            if (gameObject.activeSelf == active)
            {
                return;
            }
            gameObject.SetActive(active);
        }

#if UNITY_EDITOR
        public static void DisplayEditorProgressBar(string title, string info, int currentStep, int totalStep)
        {
            EditorUtility.DisplayProgressBar(title, info, currentStep / (float)totalStep);
        }
#endif
    }
}
