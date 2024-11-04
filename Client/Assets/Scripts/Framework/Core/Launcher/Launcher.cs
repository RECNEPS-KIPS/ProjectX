// author:KIPKIPS
// describe:启动器

using UnityEngine;
using Framework.Core.Manager.Store;
using Framework.Core.Manager.AnitCheat;
using Framework.Core.Manager.Config;
using Framework.Core.Manager.Language;
using Framework.Core.Manager.ResourcesLoad;
using Framework.Core.Manager.UI;
using Framework.Core.Manager;
using Framework.Core.Manager.Input;
using Framework.Core.Manager.Scene;
using Framework.Core.World;
using GamePlay.Item;
using GamePlay.Player;

namespace Framework.Core.Launcher
{
    /// <summary>
    /// 游戏启动器
    /// </summary>
    public class Launcher : MonoBehaviour
    {
        // 调用业务逻辑
        private void Awake()
        {
            //core manager先启动
            GameManager.Instance.Launch();
            InputManager.Instance.Launch();
            ResourcesLoadManager.Instance.Launch();
            ConfigManager.Instance.Launch();
            StoreManager.Instance.Launch();
            AntiCheatManager.Instance.Launch();
            LanguageManager.Instance.Launch();
            SceneManager.Instance.Launch();
            WorldManager.Instance.Launch();
            UIManager.Instance.Launch();

            //gameplay玩法相关manager后启动
            PlayerManager.Instance.Launch();
            ItemManager.Instance.Launch();
            
            UIManager.OpenUI(EUI.PlotUI);
        }
    }
}
