using System.Collections;
using System.Collections.Generic;
using GameFramework;
using UnityEngine;

namespace GameLogic
{
    public class GameRuntimeDataModule : GameLogicModule
    {
        private PlayerControllerData m_playerControllerData;
        public PlayerControllerData PlayerControllerData => m_playerControllerData;

        public void InitPlayerControllerData()
        {
            m_playerControllerData = new PlayerControllerData();
        }
    }
}

