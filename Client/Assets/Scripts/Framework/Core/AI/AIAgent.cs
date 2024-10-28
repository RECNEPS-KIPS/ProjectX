using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core.AI
{
    public class AIAgent : MonoBehaviour
    {
        private AIBrainBase m_AIBrain;

        private void Start()
        {
            m_AIBrain = new SimpleAIBrain(this);
        }

        void Update()
        {
            m_AIBrain.BrainUpdate();
        }
    }
}