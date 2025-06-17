using System;
using System.Collections.Generic;
using GameLogic;
using UnityEngine;


namespace GameFramework
{
    public class GameRoot : Singleton<GameRoot>
    {
        private GameSceneMonoBase m_currentGameSceneMono;

        public void SetCurrentGameSceneMono(GameSceneMonoBase gameSceneMono)
        {
            m_currentGameSceneMono = gameSceneMono;
        }

        public T GetGameModule<T>() where T : GameModuleBase
        {
            return m_currentGameSceneMono.GameModuleManager.GetModule<T>();
        }
    }


    public abstract class GameModuleBase
    {
        public abstract void InitModule();
        public virtual void UpdateModule() { }
        public abstract void DisposeModule();
        // 用于所有Module加载完后 某一些模块依赖其他模块进行加载初始化
        public virtual void LoadModule() { }
    }

    public abstract class GameLogicModuleBase : GameModuleBase
    {

    }

    public abstract class GameFrameworkModuleBase : GameModuleBase
    {

    }

    public class GameModuleManager
    {
        private Dictionary<Type, GameLogicModuleBase> m_gameLogicModules = new Dictionary<Type, GameLogicModuleBase>();
        private Dictionary<Type, GameFrameworkModuleBase> m_gameFrameworkModules = new Dictionary<Type, GameFrameworkModuleBase>();

        public void AddModule(GameModuleBase module)
        {
            var moduleType = module.GetType();
            if (module is GameLogicModuleBase logicModule)
            {
                m_gameLogicModules.Add(moduleType, logicModule);
            }
            else if (module is GameFrameworkModuleBase frameworkModule)
            {
                m_gameFrameworkModules.Add(moduleType, frameworkModule);
            }
            else
            {
                Debug.LogError($"模块{moduleType.Name}类型错误");
                return;
            }
            module.InitModule();
        }

        public void LoadAllModule()
        {
            var gameFrameworkModules = m_gameFrameworkModules.Values;
            var gameLogicModules = m_gameLogicModules.Values;
            foreach (var module in gameFrameworkModules)
            {
                module.LoadModule();
            }
            foreach (var module in gameLogicModules)
            {
                module.LoadModule();
            }
        }

        public void UpdateModules()
        {
            foreach (var module in m_gameLogicModules.Values)
            {
                module?.UpdateModule();
            }
            foreach (var module in m_gameFrameworkModules.Values)
            {
                module?.UpdateModule();
            }
        }


        public void DisposeModule<T>() where T : GameModuleBase
        {
            var moduleType = typeof(T);
            if (m_gameLogicModules.TryGetValue(moduleType, out var gameLogicModule))
            {
                gameLogicModule.DisposeModule();
                m_gameLogicModules.Remove(moduleType);
            }

            else if (m_gameFrameworkModules.TryGetValue(moduleType, out var frameworkModule))
            {
                frameworkModule.DisposeModule();
                m_gameFrameworkModules.Remove(moduleType);
            }
            else
            {
                Debug.LogError($"模块{moduleType.Name}不存在");
            }
        }


        public void DisposeAllModules()
        {
            foreach (var module in m_gameLogicModules.Values)
            {
                module.DisposeModule();
            }
            m_gameLogicModules.Clear();

            foreach (var module in m_gameFrameworkModules.Values)
            {
                module.DisposeModule();
            }
            m_gameFrameworkModules.Clear();
        }

        public T GetModule<T>() where T : GameModuleBase
        {
            var moduleType = typeof(T);
            if (m_gameLogicModules.TryGetValue(moduleType, out var gameLogicModule))
            {
                return gameLogicModule as T;
            }
            else if (m_gameFrameworkModules.TryGetValue(moduleType, out var gameFrameworkModule))
            {
                return gameFrameworkModule as T;
            }
            else
            {
                Debug.LogError($"模块{moduleType.Name}不存在");
                return null;
            }
        }

        public bool TryGetModule<T>(out T module) where T : GameModuleBase
        {
            module = GetModule<T>();
            if (module != null)
            {
                return true;
            }
            return false;
        }
            

    }

}