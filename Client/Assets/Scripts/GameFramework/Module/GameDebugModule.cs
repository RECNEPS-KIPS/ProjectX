using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework;
using IngameDebugConsole;
using UnityEngine;

namespace GameFramework
{
    public class GameDebugModule : GameFrameworkModule
    {
        private List<Action> m_debugActions;
        private GameObject m_ingameDebugConsoleGO;

        public GameDebugModule()
        {
            m_debugActions = new List<Action>();
            var ingameConsolePrefab  = GameResourceLoader.Instance.LoadResource<GameObject>($"IngameDebug/IngameDebugConsole");
            m_ingameDebugConsoleGO = GameObject.Instantiate(ingameConsolePrefab, null);
            m_ingameDebugConsoleGO.transform.localPosition = Vector3.zero;
        }
        
        public void RegisterUpdateAction(Action action)
        {
            m_debugActions.Add(action);
        }

        public void RemoveUpdateAction(Action action)
        {
            m_debugActions.Remove(action);
        }
        
        public void Update()
        {
            foreach (var action in m_debugActions)
            {
                action?.Invoke();
            }
        }
    }
}