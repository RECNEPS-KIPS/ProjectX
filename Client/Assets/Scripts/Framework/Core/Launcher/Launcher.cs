// author:KIPKIPS
// describe:启动器
using UnityEngine;
using Framework.Core.Manager.Store;
using Framework.Core.Manager.AnitCheat;
using Framework.Core.Manager.Language;
using Framework.Core.Manager.UI;

namespace Framework.Core.Launcher {
    public class Launcher : MonoBehaviour {
        // 调用业务逻辑
        void Awake() {
            StoreManager.Instance.Launch();
            AntiCheatManager.Instance.Launch();
            LanguageManager.Instance.Launch();
            UIManager.Instance.Launch();
        }
    }
}