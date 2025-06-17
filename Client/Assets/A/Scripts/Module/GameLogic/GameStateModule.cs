using System;
using UnityEngine;
using GameFramework;
using Player;

namespace GameLogic
{
    public enum GameState
    {
        Normal,     // 正常游戏状态
        Paused,     // 游戏暂停状态
        Loading,    // 加载状态
        GameOver    // 游戏结束状态
    }

    public class GameStateModule : GameLogicModuleBase
    {
        private GameState m_currentState = GameState.Normal;
        private PlayerMono m_playerMono;

        // 游戏状态改变事件
        public event Action<GameState> OnGameStateChanged;

        public override void InitModule()
        {
            // 初始状态为正常
            m_currentState = GameState.Normal;
        }

        public override void LoadModule()
        {
            // TODO Load Player
        }

        public override void UpdateModule()
        {
            // 在更新时可以添加根据当前状态的逻辑
        }

        public override void DisposeModule()
        {
            // 清理事件
            OnGameStateChanged = null;
        }

        /// <summary>
        /// 获取当前游戏状态
        /// </summary>
        public GameState CurrentState => m_currentState;

        /// <summary>
        /// 设置游戏状态
        /// </summary>
        /// <param name="newState">新的游戏状态</param>
        public void SetGameState(GameState newState)
        {
            // 如果状态没有变化，不做任何处理
            if (m_currentState == newState)
                return;

            // 更新状态
            var oldState = m_currentState;
            m_currentState = newState;

            // 根据新状态处理PlayerMono的输入
            HandlePlayerInput();

            // 触发状态变化事件
            OnGameStateChanged?.Invoke(newState);

            Debug.Log($"游戏状态从{oldState}变更为{newState}");
        }

        /// <summary>
        /// 根据当前状态处理玩家输入
        /// </summary>
        private void HandlePlayerInput()
        {
            if (m_playerMono == null)
                return;

            // 判断当前状态是否应该启用玩家输入
            bool enableInput = m_currentState == GameState.Normal;

            // 使用PlayerMono的InputEnabled属性控制输入状态
            m_playerMono.InputEnabled = enableInput;
        }

        /// <summary>
        /// 检查游戏状态是否为指定状态
        /// </summary>
        /// <param name="state">要检查的状态</param>
        /// <returns>是否为指定状态</returns>
        public bool IsState(GameState state)
        {
            return m_currentState == state;
        }

        /// <summary>
        /// 打开UI时调用，将游戏状态设置为UIOpened
        /// </summary>
        public void OpenUI()
        {
            //SetGameState(GameState.UIOpened);
        }

        /// <summary>
        /// 关闭UI时调用，将游戏状态设置回Normal
        /// </summary>
        public void CloseUI()
        {
            SetGameState(GameState.Normal);
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            SetGameState(GameState.Paused);
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            SetGameState(GameState.Normal);
        }
    }
} 