using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Core.Singleton;
using UnityEngine;

namespace Framework.Core.AI
{
    public interface IAIAction
    {
        void ResetAction();
    }
    
    
    public class AIActionFactory : Singleton<AIActionFactory>
    {
        private Dictionary<Type, Queue<AIActionBase>> m_actionPoolDict;

        public override void Initialize()
        {
            base.Initialize();
            m_actionPoolDict = new Dictionary<Type, Queue<AIActionBase>>();
        }

        public T GetAction<T>() where T : AIActionBase, new ()
        {
            var actionType = typeof(T);
            if (!m_actionPoolDict.TryGetValue(actionType,out var actionQueue) || actionQueue.Count == 0)
            {
                return new T();
            }
            return (T)actionQueue.Dequeue();
        }

        public void Recycle<T>(T action) where T : AIActionBase
        {
            var actionType = typeof(T);
            if (!m_actionPoolDict.TryGetValue(actionType,out var actionQueue))
            {
                actionQueue = m_actionPoolDict[actionType] = new Queue<AIActionBase>();
            }
            action.ResetAction();
            actionQueue.Enqueue(action);
        }
    }
}