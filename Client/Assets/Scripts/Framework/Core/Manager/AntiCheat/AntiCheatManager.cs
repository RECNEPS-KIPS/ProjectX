// author:KIPKIPS
// describe:反作弊

using System;
using Framework.Core.Singleton;

namespace Framework.Core.Manager.AnitCheat
{
    /// <summary>
    /// 反作弊管理器
    /// </summary>
    // [MonoSingletonPath("[Manager]/AntiCheatManager")]
    public class AntiCheatManager : Singleton<AntiCheatManager>
    {
        /// <summary>
        /// 作弊委托
        /// </summary>
        public Action OnDetected;

        /// <summary>
        /// 作弊标记
        /// </summary>
        public bool IsCheat { get; set; }

        internal void Detected()
        {
            OnDetected?.Invoke();
        }

        /// <summary>
        /// 绑定作弊触发行为
        /// </summary>
        private void MonitorCheat()
        {
            OnDetected = () =>
            {
                IsCheat = true;
                LogManager.Log("AntiCheatHelper", "存在作弊行为!");
            };
        }

        // public override void Initialize() {
        // }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Launch()
        {
            MonitorCheat();
        }
    }
}