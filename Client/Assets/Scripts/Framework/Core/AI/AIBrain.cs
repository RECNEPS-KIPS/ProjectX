
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core.AI
{
    public class GameAIConst
    {
        public static int PlayerLayer = LayerMask.NameToLayer("Player");
    }
    
    public abstract class AIBrainBase
    {
        protected AIAgent m_AIAgent;
        public AIBrainBase(AIAgent agent)
        {
            m_AIAgent = agent;
        }
        
        public abstract void BrainUpdate();
    }


    /// <summary>
    /// 包含基本巡逻、追踪敌人、攻击敌人
    /// </summary>
    public class SimpleAIBrain : AIBrainBase
    {
        // 当前action
        private Queue<AIActionBase> m_actions;
        // 巡逻地点
        private List<Transform> m_patrolPos;
        private int m_lastPatrolIndex = 0;
        
        public SimpleAIBrain(AIAgent agent) : base(agent)
        {
        }
        
        public override void BrainUpdate()
        {
            if (m_actions.Count > 0)
            {
                var curAction = m_actions.Peek();
                curAction.BeforeExecute();
                curAction.Execute(m_AIAgent,out var actionResult);
                if (actionResult.type == eActionExecuteType.SUCCESS)
                {
                    curAction.AfterExecute();
                    m_actions.Dequeue();
                }else if (actionResult.type == eActionExecuteType.BREAK)
                {
                    m_actions.Clear();
                    GenActions();
                }
            }
            else
            {
                GenActions();
            }
        }

        // TODO 对接Action 工厂
        private void GenActions()
        {
            var curPos = m_AIAgent.transform.position;
            // Idle-Partol
            if (!AISensor.TrySphereDetect(curPos, 3, GameAIConst.PlayerLayer, out var player))
            {
                m_actions.Enqueue(new IdleAction());
                m_actions.Enqueue(new WaitAction(2f));
                m_actions.Enqueue(new MoveToTargetAction(0.1f,m_patrolPos[m_lastPatrolIndex % m_patrolPos.Count]));
                m_lastPatrolIndex++;
                m_actions.Enqueue(new WaitAction(1f));
            }
            else
            {
                // Attack
                if (Vector3.Distance(curPos, player.position) >= 1)
                {
                    m_actions.Enqueue(new MoveToTargetAction(1,player));
                }
                m_actions.Enqueue(new AttackAction());
                m_actions.Enqueue(new WaitAction(2f));
            }
            
        }
    }
}