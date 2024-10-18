using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameFramework
{
    // WholeGameManager
    public class WholeGameManager
    {
        private Dictionary<Type, GameBaseModule> m_gameBaseModuleDict;
        private GameBaseScene m_currentGameScene;

        public GameBaseScene CurrentGameScene => m_currentGameScene;
        
        public void InitWholeGameManager()
        {
            m_gameBaseModuleDict = new Dictionary<Type, GameBaseModule>();
        }

        public void RegisterModuleFromScene(GameBaseScene gameBaseScene)
        {
            m_gameBaseModuleDict.Clear();

            var modules = gameBaseScene.RegisterGameModules();
            if (modules != null)
            {
                foreach (var module in modules)
                {
                    m_gameBaseModuleDict[module.GetType()] = module;
                }
            }
            m_currentGameScene = gameBaseScene;
        }

        public T GetModule<T>() where T : GameBaseModule
        {
            T targetModule = null;
            var typeKey = typeof(T);
            if (m_gameBaseModuleDict?.TryGetValue(typeKey, out var module) ?? false)
            {
                targetModule = (T)module;
            }

            Assert.IsNotNull(targetModule, $"{typeKey.FullName} 获取失败");
            return targetModule;
        }
    }
}
