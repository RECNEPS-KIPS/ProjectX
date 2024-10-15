using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Common;
using System.Reflection;
using Framework.Core.Pool;

public class LogManager {
    // Log Color HashSet防止重复颜色   例如：<"log","#ff00ff">
    private static Dictionary<string, string> _logColorDict = new Dictionary<string, string>();
    private static HashSet<string> _logColorHashSet = new HashSet<string>();
    private static BasalPool<LogEntity> _logEntityPool = new BasalPool<LogEntity>();
    private static Dictionary<int, string> _spaceDict = new Dictionary<int, string>();
    // Log
    public static void Log(params object[] messages) {
        string tag = "Log";
        if (messages == null || messages.Length == 0) {
            Debug.Log(GetLogFormatString(tag, "The expected value is null"));
            return;
        }
        ;
        int startIdx = 0;
        if (messages.Length == 1) {
            startIdx = 0;
        } else if (messages[0] is string) {
            tag = (string)messages[0];
            startIdx = 1;
        }
        string msg = "";
        LogEntity logEntity;
        for (int i = startIdx; i < messages.Length; i++) {
            logEntity = GetMessageData(messages[i]);
            msg += (logEntity.Content + "\n");
            _logEntityPool.Recycle(logEntity);
        }
        Debug.Log(GetLogFormatString(tag, msg));
    }
    private static LogEntity GetMessageData(object msgObj) {
        // messages[i]
        return HandleLogUnit(true, msgObj, -1);
    }
    private static LogEntity HandleLogUnit(bool firstLine, object msgObj, int layer) {
        string msg = "";
        bool innerLine = true;
        if (InheritInterface<IEnumerable>(msgObj) && !(msgObj is string)) {
            msg += firstLine ? "" : "\n";
            IEnumerator ie = ((IEnumerable)msgObj).GetEnumerator();
            string tempStr = "";
            int length = GetEnumeratorCount(ie);
            ie = ((IEnumerable)msgObj).GetEnumerator();
            int count = 0;
            bool last, iskvp;
            while (ie.MoveNext()) {
                if (ie.Current != null) {
                    dynamic data = ie.Current;
                    iskvp = ContainProperty(data, "Key");
                    LogEntity le = HandleLogUnit(false, (iskvp ? data.Value : data), layer + 1);
                    last = count == length - 1;
                    tempStr = (GetTable(layer + 1) + (iskvp ? data.Key : count) + " : " + le.Content) + (le.InnerLine && !last ? "\n" : "");
                    innerLine = last ? true : (innerLine ? false : true);
                    msg += tempStr;
                }
                count++;
            }
        } else {
            msg = msgObj.ToString();
        }
        LogEntity logEntity = _logEntityPool.Allocate();
        logEntity.Set(msg, innerLine);
        return logEntity;
    }
    private static int GetEnumeratorCount(IEnumerator ie) {
        int cnt = 0;
        while (ie.MoveNext()) {
            cnt++;
        }
        return cnt;
    }
    private static string GetTable(int num) {
        if (_spaceDict.ContainsKey(num)) {
            return _spaceDict[num];
        }
        string space = "";
        for (int i = 0; i < num; i++) {
            space += "    ";
        }
        _spaceDict.Add(num, space);
        return space;
    }
    private static bool ContainProperty(object obj, string propertyName) {
        if (obj != null && !string.IsNullOrEmpty(propertyName)) {
            PropertyInfo findedPropertyInfo = obj.GetType().GetProperty(propertyName);
            return (findedPropertyInfo != null);
        }
        return false;
    }
    private static bool InheritInterface<T>(object obj) {
        return typeof(T).IsAssignableFrom(obj.GetType());
    }

    // LogWarning
    public static void LogWarning(object message) {
        LogWarning("WARNING", message);
    }
    public static void LogWarning(string tag, object message) {
        Debug.LogWarning(GetLogFormatString(tag, message));
    }

    // LogError
    public static void LogError(object message) {
        LogError("ERROR", message);
    }
    public static void LogError(string tag, object message) {
        Debug.LogError(GetLogFormatString(tag, message));
    }
    private static String GetLogFormatString(string tag, object message) {
        string tempColorCode;
        if (_logColorDict.ContainsKey(tag)) {
            tempColorCode = _logColorDict[tag];
        } else {
            int count = 0; // 颜色循环次数上限
            do {
                tempColorCode = CommonUtils.GetRandomColorCode();
                count++;
                if (count > 1000) {
                    // 获取颜色次数超过1000次 默认返回白色
                    Debug.LogWarning("Color Get Duplicated");
                    return String.Format("<color=#000000>[{0}]</color>: {1}", tag, message);
                }
            } while (_logColorHashSet.Contains(tempColorCode));
            // 找到对应颜色
            _logColorDict[tag] = tempColorCode;
            _logColorHashSet.Add(tempColorCode);
        }
        return String.Format("<color={0}>[{1}]</color>: {2}", tempColorCode, tag, message);
    }
}