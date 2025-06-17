using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;

namespace GameLogic
{    
    /// <summary>
    /// 游戏事件模块 - 提供全局事件系统
    /// </summary>
    public class GameEventModule : GameLogicModuleBase
    {
        // 使用字典存储所有事件及其对应的委托
        private Dictionary<GameEventType, Action<GameEventArgBase>> m_eventDict;

        public override void InitModule()
        {
            // 初始化事件字典
            m_eventDict = new Dictionary<GameEventType, Action<GameEventArgBase>>();
        }

        public override void LoadModule()
        {
            // 加载模块时的额外初始化
        }


        public override void DisposeModule()
        {
            // 清空所有事件
            m_eventDict.Clear();
        }

        public void RegisterEvent(GameEventType eventType, Action<GameEventArgBase> callback)
        {
            if (eventType == GameEventType.None)
            {
                Debug.LogError("事件类型不能为None");
                return;
            }

            if(m_eventDict.ContainsKey(eventType))
            {
                m_eventDict[eventType] += callback;
            }
            else
            {
                m_eventDict[eventType] = callback;
            }
        }


        public void UnregisterEvent(GameEventType eventType, Action<GameEventArgBase> callback)
        {
            if (eventType == GameEventType.None)
            {
                Debug.LogError("事件类型不能为None");
                return;
            }

            if(m_eventDict.ContainsKey(eventType))
            {
                m_eventDict[eventType] -= callback;
            }
        }

        public void DispatchEvent<T>(GameEventType eventType, T eventArg) where T : GameEventArgBase
        {
            if (eventType == GameEventType.None)
            {
                Debug.LogError("事件类型不能为None");
                return;
            }

            if (m_eventDict.ContainsKey(eventType))
            {
                m_eventDict[eventType]?.Invoke(eventArg);
            }           
        }

        public void DispatchEvent(GameEventType eventType)
        {
            DispatchEvent<GameEventArgBase>(eventType, null);
        }
    }
} 