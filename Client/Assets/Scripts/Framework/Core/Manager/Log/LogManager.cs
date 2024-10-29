using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Common;
using Framework.Core.Pool;

/// <summary>
/// 日志管理器
/// </summary>
public static class LogManager
{
    // Log Color HashSet防止重复颜色   例如：<"log","#ff00ff">
    private static readonly Dictionary<string, string> LOGColorDict = new Dictionary<string, string>();
    private static readonly HashSet<string> LOGColorHashSet = new HashSet<string>();
    private static readonly BasalPool<LogEntity> LOGEntityPool = new BasalPool<LogEntity>();
    private static readonly Dictionary<int, string> SpaceDict = new Dictionary<int, string>();

    /// <summary>
    /// 输出日志
    /// </summary>
    /// <param name="messages"></param>
    public static void Log(params object[] messages)
    {
        var tag = "Log";
        if (messages == null || messages.Length == 0)
        {
            Debug.Log(GetLogFormatString(tag, "The expected value is null"));
            return;
        }

        var startIdx = 0;
        if (messages.Length == 1)
        {
            startIdx = 0;
        }
        else if (messages[0] is string)
        {
            tag = (string)messages[0];
            startIdx = 1;
        }

        var msg = "";
        for (var i = startIdx; i < messages.Length; i++)
        {
            var logEntity = GetMessageData(messages[i]);
            msg += logEntity.Content + "\n";
            LOGEntityPool.Recycle(logEntity);
        }

        Debug.Log(GetLogFormatString(tag, msg));
    }

    private static LogEntity GetMessageData(object msgObj)
    {
        // messages[i]
        return HandleLogUnit(true, msgObj, -1);
    }

    private static LogEntity HandleLogUnit(bool firstLine, object msgObj, int layer)
    {
        var msg = "";
        var innerLine = true;
        if (InheritInterface<IEnumerable>(msgObj) && msgObj is not string)
        {
            msg += firstLine ? "" : "\n";
            var ie = ((IEnumerable)msgObj).GetEnumerator();
            using var unknown = ie as IDisposable;
            var length = GetEnumeratorCount(ie);
            ie = ((IEnumerable)msgObj).GetEnumerator();
            var count = 0;
            while (ie.MoveNext())
            {
                if (ie.Current != null)
                {
                    dynamic data = ie.Current;
                    bool iskvp = ContainProperty(data, "Key");
                    LogEntity le = HandleLogUnit(false, (iskvp ? data.Value : data), layer + 1);
                    var last = count == length - 1;
                    string tempStr = (GetTable(layer + 1) + (iskvp ? data.Key : count) + " : " + le.Content) +
                                     (le.InnerLine && !last ? "\n" : "");
                    innerLine = last || !innerLine;
                    msg += tempStr;
                }

                count++;
            }
        }
        else
        {
            msg = msgObj.ToString();
        }

        var logEntity = LOGEntityPool.Allocate();
        logEntity.Set(msg, innerLine);
        return logEntity;
    }

    private static int GetEnumeratorCount(IEnumerator ie)
    {
        var cnt = 0;
        while (ie.MoveNext())
        {
            cnt++;
        }

        return cnt;
    }

    private static string GetTable(int num)
    {
        if (SpaceDict.TryGetValue(num, out var table))
        {
            return table;
        }

        var space = "";
        for (var i = 0; i < num; i++)
        {
            space += "    ";
        }

        SpaceDict.Add(num, space);
        return space;
    }

    private static bool ContainProperty(object obj, string propertyName)
    {
        if (obj == null || string.IsNullOrEmpty(propertyName)) return false;
        var findedPropertyInfo = obj.GetType().GetProperty(propertyName);
        return (findedPropertyInfo != null);
    }

    private static bool InheritInterface<T>(object obj)
    {
        // return typeof(T).IsAssignableFrom(obj.GetType());
        return obj is T;
    }

    /// <summary>
    /// 警告日志输出
    /// </summary>
    /// <param name="message"></param>
    public static void LogWarning(object message)
    {
        LogWarning("WARNING", message);
    }

    /// <summary>
    /// 警告日志输出 Tag
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="message"></param>
    public static void LogWarning(string tag, object message)
    {
        Debug.LogWarning(GetLogFormatString(tag, message));
    }

    /// <summary>
    /// 报错日志输出
    /// </summary>
    /// <param name="message"></param>
    public static void LogError(object message)
    {
        LogError("ERROR", message);
    }

    /// <summary>
    /// 报错日志输出 Tag
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="message"></param>
    public static void LogError(string tag, object message)
    {
        Debug.LogError(GetLogFormatString(tag, message));
    }

    private static string GetLogFormatString(string tag, object message)
    {
        string tempColorCode;
        if (LOGColorDict.TryGetValue(tag, out var value))
        {
            tempColorCode = value;
        }
        else
        {
            var count = 0; // 颜色循环次数上限
            do
            {
                tempColorCode = CommonUtils.GetRandomColorCode();
                count++;
                if (count <= 1000) continue;
                // 获取颜色次数超过1000次 默认返回白色
                Debug.LogWarning("Color Get Duplicated");
                return $"<color=#000000>[{tag}]</color>: {message}";
            } while (LOGColorHashSet.Contains(tempColorCode));

            // 找到对应颜色
            LOGColorDict[tag] = tempColorCode;
            LOGColorHashSet.Add(tempColorCode);
        }

        return $"<color={tempColorCode}>[{tag}]</color>: {message}";
    }
}