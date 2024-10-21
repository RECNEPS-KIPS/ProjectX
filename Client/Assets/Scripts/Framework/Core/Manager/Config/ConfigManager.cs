// author:KIPKIPS
// describe:配置表管理,负责加载和解析配置表

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Framework.Core.Singleton;
using Framework.Common;
using Framework.Core.Container;

namespace Framework.Core.Manager.Config
{
    public enum ConfigNameDef
    {
        Color = 0,
        StartStringTable = 1,
        LevelSetting = 2,
        Scene = 3,
        ColorDef = 4,
    }
    /// <summary>
    /// 配置管理器
    /// </summary>
    // [MonoSingletonPath("[Manager]/ConfigManager")]
    public class ConfigManager : Singleton<ConfigManager>
    {
        private static readonly Dictionary<ConfigNameDef, string> ConfigNameDict = new()
        {
            {ConfigNameDef.Color, "Color" },
            {ConfigNameDef.StartStringTable,"StartStringTable" },
            {ConfigNameDef.LevelSetting,"LevelSetting"},
            {ConfigNameDef.Scene,"Scene"},
            {ConfigNameDef.ColorDef,"ColorDef"}
        };
        
        private const string LOGTag = "ConfigManager";
        private const string ConfigPath = "Config/"; //配置表路径

        private static readonly RestrictedDictionary<string, List<dynamic>> _configDict = new (); //配置总表

        private static readonly RestrictedDictionary<string, RestrictedDictionary<string, string>> _typeDict = new ();

        /// <summary>
        /// 
        /// </summary>
        public void Launch()
        {
            LogManager.Log(LOGTag, "ConfigManager Launch");
            AnalyticsConfig();
        }

        // 解析配置表
        private void AnalyticsConfig()
        {
            _configDict.EnableWrite();
            _typeDict.EnableWrite();
            //TODO:配置表加载优化,目前是全部加载
            // _configDict = new Dictionary<string, List<dynamic>>();
            //获取所有配置表
            // UIUtils.LoadJsonByPath<List<JObject>>("Data/" + tabName + ".json");
            var dir = new DirectoryInfo(ConfigPath);
            var files = dir.GetFileSystemInfos();
            foreach (var t in files)
            {
                var configName = t.Name.Replace(".json", "");
                // fullName = configPath + files[i].Name;
                if (_configDict.ContainsKey(configName)) continue;
                _configDict.Add(configName, new List<dynamic>());
                // _configDict[configName].Add(null); //预留一个位置
                // configDict[configName].Add();
                LogManager.Log(LOGTag, "Load Config:", t.Name);
                try
                {
                    var jObjList = JsonUtils.LoadJsonByPath<List<JObject>>(ConfigPath + t.Name);
                    var metatable = jObjList[^1];
                    if (metatable.ContainsKey("__metatable"))
                    {
                        var metatableProperties = metatable.Properties();
                        if (!_typeDict.ContainsKey(configName))
                        {
                            _typeDict.Add(configName, new RestrictedDictionary<string, string>());
                        }

                        foreach (var metatableProp in metatableProperties)
                        {
                            if (metatableProp.Name == "__metatable") continue;
                            _typeDict[configName].EnableWrite();
                            _typeDict[configName].Add(metatableProp.Name, metatableProp.Value.ToString());
                            _typeDict[configName].ForbidWrite();
                        }

                        for (var j = 0; j < jObjList.Count - 1; j++)
                        {
                            var table = new RestrictedDictionary<string, dynamic>();
                            table.EnableWrite();
                            var properties = jObjList[j].Properties();
                            foreach (var prop in properties)
                            {
                                table[prop.Name] = prop.Value.Type.ToString() switch
                                {
                                    "Integer" => (int)prop.Value,
                                    "Float" => (float)prop.Value,
                                    "Boolean" => (bool)prop.Value,
                                    "String" => prop.Value.ToString(),
                                    "Array" => HandleArray(prop.Value.ToArray()),
                                    "Object" => HandleDict(prop.Value.ToObject<JObject>(), prop.Name, configName),
                                    _ => table[prop.Name]
                                };
                            }

                            table.ForbidWrite();
                            _configDict[configName].Add(table);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Log(LOGTag, configName);
                    LogManager.LogError(LOGTag, ex.ToString());
                }
            }

            _configDict.ForbidWrite();
            _typeDict.ForbidWrite();
            LogManager.Log(LOGTag, "Config table data is parsed");
        }

        /// <summary>
        /// 处理字典类型的配置表
        /// </summary>
        /// <param name="jObj"></param>
        /// <param name="filedName"></param>
        /// <param name="cfName"></param>
        /// <returns></returns>
        private dynamic HandleDict(JObject jObj, string filedName, string cfName)
        {
            dynamic table = new RestrictedDictionary<dynamic, dynamic>();
            table.EnableWrite();
            var valueTypeDict = _typeDict[cfName];
            var properties = jObj.Properties();
            dynamic key = null;
            foreach (var prop in properties)
            {
                if (valueTypeDict.ContainsKey(filedName))
                {
                    if (valueTypeDict[filedName].StartsWith("dict<int"))
                    {
                        key = int.Parse(prop.Name);
                    }
                    else if (valueTypeDict[filedName].StartsWith("dict<float"))
                    {
                        key = float.Parse(prop.Name);
                    }
                    else if (valueTypeDict[filedName].StartsWith("dict<string"))
                    {
                        key = prop.Name;
                    }

                    switch (prop.Value.Type.ToString())
                    {
                        case "Integer":
                            table.Add(key, (int)prop.Value);
                            break;
                        case "Float":
                            table.Add(key, (float)prop.Value);
                            break;
                        case "Boolean":
                            table.Add(key, (bool)prop.Value);
                            break;
                        case "String":
                            table.Add(key, prop.Value.ToString());
                            break;
                    }
                }
            }

            table.ForbidWrite();
            // LogManager.Log(logTag, table);    
            return table;
        }

        // 递归处理数组类型
        private static dynamic HandleArray(IReadOnlyList<JToken> array)
        {
            dynamic table = new RestrictedDictionary<int, dynamic>();
            table.EnableWrite();
            for (var i = 0; i <= array.Count; i++)
            {
                switch (array[i].Type.ToString())
                {
                    case "Integer":
                        table.Add(i, (int)array[i]);
                        break;
                    case "Float":
                        table.Add(i, (float)array[i]);
                        break;
                    case "Boolean":
                        table.Add(i, (bool)array[i]);
                        break;
                    case "String":
                        table.Add(i, array[i].ToString());
                        break;
                    case "Array":
                        table.Add(i, HandleArray(array[i].ToArray()));
                        break;
                }
            }

            table.ForbidWrite();
            return table;
        }

        /// <summary>
        /// 获取配置表
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static List<dynamic> GetConfig(ConfigNameDef configName)
        {
            try
            {
                if (ConfigNameDict.TryGetValue(configName, out var name))
                {
                    if (_configDict.TryGetValue(name, out var config))
                    {
                        return config;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }


        /// <summary>
        /// 获取配置表的指定id的Hashtable
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static dynamic GetConfigByID(ConfigNameDef configName, int id)
        {
            try
            {
                if (ConfigNameDict.TryGetValue(configName, out var name))
                {
                    if (_configDict.ContainsKey(name))
                    {
                        if (_configDict[name] != null && _configDict[name][id - 1] != null)
                        {
                            return _configDict[name][id - 1];
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }
    }
}