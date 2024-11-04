using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
    
    public abstract class AIActionBase : IAIAction
    {
        protected Func<bool> m_checkBreakFunc;

        public virtual void BeforeExecute()
        {
            
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual void Execute(AIAgent AIAgent, out SActionResult result)
        {
            result = new SActionResult()
            {
                type = eActionExecuteType.EXECUTING,
            };
            if (m_checkBreakFunc?.Invoke() ?? false)
            {
                result.type = eActionExecuteType.BREAK;
            }
        }

        public virtual void AfterExecute()
        {
            
        }

        public void RegisterCheckBreakFunc(Func<bool> checkFunc)
        {
            m_checkBreakFunc += checkFunc;
        }

        public void RemoveCheckBreakFunc(Func<bool> checkFunc)
        {
            m_checkBreakFunc -= checkFunc;
        }

        public virtual void ResetAction()
        {
            
        }
        
#if UNITY_EDITOR
        public void DrawDebugAction()
        {
            EditorGUILayout.LabelField($"ActionName:{this.ToString()}");
        }
#endif
    }

    /// <summary>
    /// 能被打断的WaitAction
    /// </summary>
    public class WaitAction : AIActionBase
    {
        private float m_timer;

        public WaitAction SetTimer(float timer)
        {
            m_timer = timer;
            return this;
        }

        public override void Execute(AIAgent AIAgent, out SActionResult result)
        {            
            base.Execute(AIAgent,out result);
            if (result.type == eActionExecuteType.BREAK)
            {
                return;
            }
            
            if (m_timer > 0)
            {
                m_timer -= Time.deltaTime;
            }
            else
            {
                result.type = eActionExecuteType.SUCCESS;
            }
        }
    }

    /// <summary>
    /// 闲置Action
    /// </summary>
    public class IdleAction : AIActionBase
    {
        public override void Execute(AIAgent AIAgent, out SActionResult result)
        {
            base.Execute(AIAgent,out result);
            if (result.type == eActionExecuteType.BREAK)
                return;
            result.type = eActionExecuteType.SUCCESS;
        }
    }

    /// <summary>
    /// 移动至目标Action
    /// </summary>
    public class MoveToTargetAction : AIActionBase
    {
        private float m_distanceOffset;
        private Transform m_target;

        public MoveToTargetAction SetMoveTarget(float distanceOffset,Transform target)
        {
            m_distanceOffset = distanceOffset;
            m_target = target;
            return this;
        }
        
        public override void Execute(AIAgent AIAgent, out SActionResult result)
        {
            base.Execute(AIAgent,out result);
            if (result.type == eActionExecuteType.BREAK)
                return;

            var curPos = AIAgent.transform.position;
            var targetPos = m_target.position;
            if (Vector3.Distance(curPos, targetPos) <= m_distanceOffset)
            {
                result.type = eActionExecuteType.SUCCESS;
            }
        }
    }
    
    public class AttackAction : AIActionBase
    {
        public override void Execute(AIAgent AIAgent, out SActionResult result)
        {
            base.Execute(AIAgent,out result);
            if (result.type == eActionExecuteType.BREAK)
                return;

            Debug.LogError("Attack");
        }
    }
}

