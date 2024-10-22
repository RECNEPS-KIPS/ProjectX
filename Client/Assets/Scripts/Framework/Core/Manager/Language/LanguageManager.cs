// author:KIPKIPS
// date:2022.06.19 18:10
// describe:多语言系统

using System.Collections.Generic;
using Framework.Core.Singleton;
using Framework.Core.Manager.Config;

namespace Framework.Core.Manager.Language
{
    /// <summary>
    /// 多语言管理器
    /// </summary>
    // [MonoSingletonPath("[Manager]/LanguageManager")]
    public class LanguageManager : Singleton<LanguageManager>
    {
        private const string LOGTag = "LanguageManager";

        public enum EStringTable
        {
            Start,
        }

        /// <summary>
        /// 语言类型
        /// </summary>
        public LanguageType Language { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Initialize()
        {
            Language = LanguageType.English;
        }

        private struct LanguageUnit
        {
            public readonly string EN;
            public readonly string SC;

            public LanguageUnit(string en, string sc)
            {
                EN = en;
                SC = sc;
            }
        }

        private readonly Dictionary<EStringTable, Dictionary<string, LanguageUnit>> _map = new();

        /// <summary>
        /// 
        /// </summary>
        public void Launch()
        {
            // var cf = ConfigManager.Instance.GetConfig("language");
            // for (var i = 1; i < cf.Count; i++) {
            //     _map.Add(cf[i]["textKey"], new LanguageUnit(cf[i]["en"], cf[i]["sc"]));
            // }
            // LogManager.Log(LOGTag, "The language configuration is loaded");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringTableType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetText(EStringTable stringTableType, string key)
        {
            LanguageUnit unit = default;
            var find = false;
            switch (stringTableType)
            {
                case EStringTable.Start:
                    if (_map.ContainsKey(EStringTable.Start) && _map[EStringTable.Start].ContainsKey(key))
                    {
                        unit = _map[EStringTable.Start][key];
                        find = true;
                    }
                    else
                    {
                        var stringTable = ConfigManager.GetConfig(EConfig.StartStringTable);
                        _map.Add(EStringTable.Start, new Dictionary<string, LanguageUnit>());
                        
                        LogManager.Log(LOGTag, "stringTable Count",stringTable.Count);
                        for (var i = 0; i < stringTable.Count; i++)
                        {
                            LogManager.Log(LOGTag, $"stringTable {i}",stringTable[i]);
                            _map[EStringTable.Start].Add(stringTable[i]["textKey"], new LanguageUnit(stringTable[i]["en"], stringTable[i]["sc"]));
                        }

                        unit = _map[EStringTable.Start][key];
                        find = true;
                    }

                    break;
                default:
                    LogManager.LogError(LOGTag, "StringTableType not defined!");
                    break;
            }

            if (find)
            {
                return Language switch
                {
                    LanguageType.English => unit.EN,
                    LanguageType.SimplifiedChinese => unit.SC,
                    _ => string.Empty
                };
            }

            return string.Empty;
        }
    }
}