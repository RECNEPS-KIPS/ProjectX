// author:KIPKIPS
// describe:反作弊
using System;
using Framework.Core.Singleton;

namespace Framework.Core.Manager.AnitCheat {
    [MonoSingletonPath("[Manager]/AntiCheatManager")]
    public class AntiCheatManager : MonoSingleton<AntiCheatManager> {
        public Action OnDetected;
        public bool IsCheat { get; set; }
        internal void Detected() {
            OnDetected?.Invoke();
        }
        void MonitorCheat() {
            OnDetected = () => {
                IsCheat = true;
                LogManager.Log("AntiCheatHelper", "存在作弊行为!");
            };
        }
        // public override void Initialize() {
        // }
        public void Launch() {
            MonitorCheat();
        }
    }
}