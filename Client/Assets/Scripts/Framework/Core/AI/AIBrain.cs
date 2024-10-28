
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core.AI
{
    public abstract class AIBrainBase
    {
        public abstract void BrainUpdate();
    }

    
    /// <summary>
    /// 包含基本巡逻、追踪敌人、攻击敌人
    /// </summary>
    public class SimpleAIBrain : AIBrainBase
    {
        public override void BrainUpdate()
        {
            
        }
    }
}