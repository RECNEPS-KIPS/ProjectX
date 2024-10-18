using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class GameRoot : MonoBehaviour
    {
        private static GameRoot m_instance;
        public static GameRoot Instance
        {
            get
            {
                if (m_instance == null)
                {
                    GameObject go = new GameObject("GameRoot");
                    m_instance = go.AddComponent<GameRoot>();
                    m_instance.GameRootInit();
                }
                return m_instance;
            }
        }

        private WholeGameManager m_wholeGameManager;

        private void Awake()
        {
            if (m_instance == null)
            {
                m_instance = this;
                GameRootInit();
            }
        }


        private void GameRootInit()
        {
            m_instance = this;
            InitGameLauncher();
            DontDestroyOnLoad(gameObject);
        }
        
        public T GetModule<T>() where T : GameBaseModule
        {
            return m_wholeGameManager.GetModule<T>();
        }

        public T GetCurrentScene<T>() where T : GameBaseScene
        {
            return m_wholeGameManager.CurrentGameScene as T;
        }
        
        public void RegisterModuleFromScene(GameBaseScene gameBaseScene)
        {
            m_wholeGameManager.RegisterModuleFromScene(gameBaseScene);
        }

        #region Init
        // 游戏初始化
        private void InitGameLauncher()
        {
            m_wholeGameManager = new WholeGameManager();
            m_wholeGameManager.InitWholeGameManager();
        }
        #endregion
    }
}