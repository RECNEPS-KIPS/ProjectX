using System.Collections;
using System.Collections.Generic;
using GameFramework;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace GameLogic
{
    public abstract class GameSceneMonoBase : MonoBehaviour
    {
        protected GameModuleManager m_gameModuleManager;
        public GameModuleManager GameModuleManager => m_gameModuleManager;

        protected virtual void Awake()
        {
            GameRoot.Instance.SetCurrentGameSceneMono(this);
            m_gameModuleManager = new GameModuleManager();
        }

        protected virtual void OnDestroy()
        {
            m_gameModuleManager.DisposeAllModules();
            GameRoot.Instance.SetCurrentGameSceneMono(null);
        }
    }
}

