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
        [SerializeField] private List<Transform> m_patrolPos;

        private void Start()
        {
            var brain = new SimpleAIBrain(this);
            m_AIBrain = brain;

            brain.SetPatrolPos(m_patrolPos);
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
            if (isHasBrain && brain != null)
            {
                DrawBrain(brain);
            }
            
        }

        private void DrawBrain(AIBrainBase brain)
        {
            var refType = new RefType<AIBrainBase>(brain);
            bool isHasActions = refType.TryGetField<Queue<AIActionBase>>("m_actions",out var acitons);
            if (isHasActions && acitons!= null)
            {   
                foreach (var action in acitons)
                {
                   action.DrawDebugAction();
                }
            }
        }
    }
#endif
}