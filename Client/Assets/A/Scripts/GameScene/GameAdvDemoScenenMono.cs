using System.Collections;
using System.Collections.Generic;
using GameFramework;
using UnityEngine;

namespace GameLogic
{
    public class GameAdvDemoScenenMono : GameSceneMonoBase
    {
        protected override void Awake()
        {
            base.Awake();

            m_gameModuleManager.AddModule(new GameResourceLoadModule());
            m_gameModuleManager.AddModule(new GameConfigModule());
        }

        protected void Start()
        {
            var gameConfigModule =  GameRoot.Instance.GetGameModule<GameConfigModule>();
            gameConfigModule.LoadConfig();
        }
    }   
}
