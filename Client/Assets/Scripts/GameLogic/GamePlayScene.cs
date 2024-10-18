using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework;
using UnityEngine;

namespace GameLogic
{
    public class GamePlayScene : GameBaseScene
    {
        private GameInputModule m_gameInputModule;
        private GameRuntimeDataModule m_gameRuntimeDataModule;
        
        public override List<GameBaseModule> RegisterGameModules()
        {
            m_gameInputModule = new GameInputModule();
            m_gameRuntimeDataModule = new GameRuntimeDataModule();
            
            var allGameMoules = new List<GameBaseModule>();
            allGameMoules.Add(m_gameInputModule);
            allGameMoules.Add(m_gameRuntimeDataModule);
            return allGameMoules;
        }

        protected override void Start()
        {
            base.Start();
            m_gameInputModule.SetInputModel(eInputModel.PlayerControllerInput,true);
            m_gameRuntimeDataModule.InitPlayerControllerData();
        }

        private void Update()
        {
            m_gameInputModule.Update();
        }
    }
}

