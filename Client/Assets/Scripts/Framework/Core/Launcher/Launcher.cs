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
            GameManager.Instance.Launch();
            ResourcesLoadManager.Instance.Launch();
            ConfigManager.Instance.Launch();
            StoreManager.Instance.Launch();
            AntiCheatManager.Instance.Launch();
            LanguageManager.Instance.Launch();
            UIManager.Instance.Launch();

            UIManager.Instance.OpenUI(UIDef.PlotUI);
        }
    }
}