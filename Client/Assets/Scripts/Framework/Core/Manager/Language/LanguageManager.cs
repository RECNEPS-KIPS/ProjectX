// author:KIPKIPS
// date:2022.06.19 18:10
// describe:多语言系统
using System.Collections.Generic;
using Framework.Core.Singleton;
using Framework.Core.Manager.Config;
namespace Framework.Core.Manager.Language {
    /// <summary>
    /// 多语言管理器
    /// </summary>
    [MonoSingletonPath("[Manager]/LanguageManager")]
    public class LanguageManager : MonoSingleton<LanguageManager> {
        private const string LOGTag = "LanguageManager";

        /// <summary>
        /// 语言类型
        /// </summary>
        public LanguageType Language { get; set; }
        /// <summary>
        /// 初始化
        /// </summary>
        public override void Initialize() {
            Language = LanguageType.English;
        }

        private struct LanguageUnit {
            public readonly string EN;
            public readonly string SC;
            public LanguageUnit(string en, string sc) {
                EN = en;
                SC = sc;
            }
        }
        private readonly Dictionary<string, LanguageUnit> _map = new Dictionary<string, LanguageUnit>();
        /// <summary>
        /// 
        /// </summary>
        public void Launch() {
            var cf = ConfigManager.Instance.GetConfig("language");
            for (var i = 1; i < cf.Count; i++) {
                _map.Add(cf[i]["textKey"], new LanguageUnit(cf[i]["en"], cf[i]["sc"]));
            }
            LogManager.Log(LOGTag, "The language configuration is loaded");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetText(string key)
        {
            var ctx = Language switch
            {
                LanguageType.English => _map[key].EN,
                LanguageType.SimplifiedChinese => _map[key].SC,
                _ => string.Empty
            };
            return ctx;
        }
    }
}