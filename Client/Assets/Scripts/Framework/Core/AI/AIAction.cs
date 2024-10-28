using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core.AI
{
    public enum eActionExecuteType
    {
        NONE,       // 未定义
        EXECUTING,  // 执行中
        SUCCESS,    // 成功
        FAILURE,    // 失败
        BREAK,  // 打断
    }
    
    public struct SActionResult
    {
        public eActionExecuteType type;
    }
    
    public abstract class AIActionBase
    {
        private Func<bool> m_checkBreakFunc;
        public abstract void BeforeExecute();
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public abstract void Execute(out SActionResult result);
        public abstract void AfterExecute();

        public void RegisterCheckBreakFunc(Func<bool> checkFunc)
        {
            m_checkBreakFunc += checkFunc;
        }

        public void RemoveCheckBreakFunc(Func<bool> checkFunc)
        {
            m_checkBreakFunc -= checkFunc;
        }
    }
}

