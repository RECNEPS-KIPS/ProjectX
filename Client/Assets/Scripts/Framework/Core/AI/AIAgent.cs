using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif
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

#if UNITY_EDITOR
    [CustomEditor(typeof(AIAgent))]
    public class AIAgentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            AIAgent aiAgent = (AIAgent)target;
            var refType = new RefType<AIAgent>(aiAgent);
            bool isHasBrain = refType.TryGetField<AIBrainBase>("m_AIBrain", out var brain);
        }
    }
#endif
}