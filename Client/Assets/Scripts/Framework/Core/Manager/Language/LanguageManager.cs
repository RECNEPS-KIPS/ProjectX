// author:KIPKIPS
// date:2022.06.19 18:10
// describe:多语言系统
using System.Collections.Generic;
using Framework.Core.Singleton;
using Framework.Core.Manager.Config;
namespace Framework.Core.Manager.Language {
    [MonoSingletonPath("[Manager]/LanguageManager")]
    public class LanguageManager : MonoSingleton<LanguageManager> {
        private string logTag = "LanguageManager";
        public LanguageType Language { get; set; }
        public override void Initialize() {
            Language = LanguageType.English;
        }
        struct LanguageUnit {
            public string en;
            public string sc;
            public LanguageUnit(string _en, string _sc) {
                en = _en;
                sc = _sc;
            }
        }
        private Dictionary<string, LanguageUnit> _map = new Dictionary<string, LanguageUnit>();
        public void Launch() {
            var cf = ConfigManager.Instance.GetConfig("language");
            for (int i = 1; i < cf.Count; i++) {
                _map.Add(cf[i]["textKey"], new LanguageUnit(cf[i]["en"], cf[i]["sc"]));
            }
            LogManager.Log(logTag, "The language configuration is loaded");
        }
        public string GetText(string key) {
            string ctx = string.Empty;
            switch (Language) {
                case LanguageType.English:
                    ctx = _map[key].en;
                    break;
                case LanguageType.SimplifiedChinese:
                    ctx = _map[key].sc;
                    break;
            }
            return ctx;
        }
    }
}