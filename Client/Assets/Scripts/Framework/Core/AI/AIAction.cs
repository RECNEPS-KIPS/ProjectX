using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core.AI
{
    public abstract class BaseAIAction
    {
        public abstract void BeforeExecute();
        public abstract void Execute();
        public abstract void AfterExecute();
    }
}

