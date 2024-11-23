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
    /// <summary>
    /// 配置管理器
    /// </summary>
    // [MonoSingletonPath("[Manager]/ConfigManager")]
    public class ConfigManager : Singleton<ConfigManager>
    {
        private static readonly Dictionary<EConfig, string> ConfigNameDict = new()
        {
            { EConfig.Color, "Color" },
            { EConfig.StartStringTable, "StartStringTable" },
            { EConfig.LevelSetting, "LevelSetting" },
            { EConfig.Scene, "Scene" },
            { EConfig.ColorDef, "ColorDef" },
            { EConfig.Character, "Character" },
            { EConfig.GrowthTemp, "GrowthTemp" },
            { EConfig.Item,"Item"},
            { EConfig.World,"World"},
        };

        private const string LOGTag = "ConfigManager";
        private const string ConfigPath = "Config/"; //配置表路径

        private static readonly RestrictedDictionary<string, List<dynamic>> _configDict = new(); //配置总表

        private static readonly RestrictedDictionary<string, RestrictedDictionary<string, string>> _typeDict = new();

        /// <summary>
        /// 
        /// </summary>
        public void Launch()
        {
            LogManager.Log(LOGTag, "ConfigManager Launch");
            AnalyticsConfig();
        }

        // 解析配置表
        public static void AnalyticsConfig()
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
                // LogManager.Log(LOGTag, "Load Config:", t.Name);
                // try
                // {
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
                        // LogManager.Log(LOGTag,"_typeDict",configName,_typeDict);

                        for (var j = 0; j < jObjList.Count - 1; j++)
                        {
                            var table = new RestrictedDictionary<string, dynamic>();
                            table.EnableWrite();
                            var properties = jObjList[j].Properties();
                            foreach (var prop in properties)
                            {
                                // if (configName == "Item")
                                // {
                                //     LogManager.Log(LOGTag, configName, _typeDict[configName][prop.Name], prop.Value == null);
                                // }

                                switch (_typeDict[configName][prop.Name])
                                {
                                    case "int":
                                        // int intV = 0;
                                        table[prop.Name] = (int)prop.Value;
                                        break;
                                    case "float":
                                        table[prop.Name] = (float)prop.Value;
                                        break;
                                    case "bool":
                                        table[prop.Name] = (bool)prop.Value;
                                        break;
                                    case "string":
                                        table[prop.Name] = prop.Value.ToString();
                                        break;
                                    case "vector2":
                                        var v2 = prop.Value.ToArray();
                                        var v2o = UnityEngine.Vector2.zero;
                                        if (v2.Length == 2)
                                        {
                                            v2o.x = (float)v2[0];
                                            v2o.y = (float)v2[1];
                                        }
                                        table[prop.Name] = v2o;
                                        break;
                                    case "vector3":
                                        var v3 = prop.Value.ToArray();
                                        var v3o = UnityEngine.Vector3.zero;
                                        if (v3.Length == 3)
                                        {
                                            v3o.x = (float)v3[0];
                                            v3o.y = (float)v3[1];
                                            v3o.z = (float)v3[2];
                                        }
                                        table[prop.Name] = v3o;
                                        break;
                                    case "list<int>":
                                        table[prop.Name] = HandleArray<int>(prop.Value.ToArray());
                                        break;
                                    case "list<float>":
                                        table[prop.Name] = HandleArray<float>(prop.Value.ToArray());
                                        break;
                                    case "list<bool>":
                                        table[prop.Name] = HandleArray<bool>(prop.Value.ToArray());
                                        break;
                                    case "list<string>":
                                        table[prop.Name] = HandleArray<string>(prop.Value.ToArray());
                                        break;
                                    case "list<list<int>>":
                                        table[prop.Name] = HandleArray<int>(prop.Value.ToArray(),2);
                                        break;
                                    case "list<list<float>>":
                                        table[prop.Name] = HandleArray<float>(prop.Value.ToArray(),2);
                                        break;
                                    case "list<list<bool>>":
                                        table[prop.Name] = HandleArray<bool>(prop.Value.ToArray(),2);
                                        break;
                                    case "list<list<string>>":
                                        table[prop.Name] = HandleArray<string>(prop.Value.ToArray(),2);
                                        break;
                                    case "dict<int,int>":
                                        table[prop.Name] = HandleDict<int, int>(prop.Value.ToObject<JObject>());
                                        break;
                                    case "dict<int,float>":
                                        table[prop.Name] = HandleDict<int, float>(prop.Value.ToObject<JObject>());
                                        break;
                                    case "dict<int,bool>":
                                        table[prop.Name] = HandleDict<int, bool>(prop.Value.ToObject<JObject>());
                                        break;
                                    case "dict<int,string>":
                                        table[prop.Name] = HandleDict<int, string>(prop.Value.ToObject<JObject>());
                                        break;
                                    case "dict<float,int>":
                                        table[prop.Name] = HandleDict<float, int>(prop.Value.ToObject<JObject>());
                                        break;
                                    case "dict<float,float>":
                                        table[prop.Name] = HandleDict<float, float>(prop.Value.ToObject<JObject>());
                                        break;
                                    case "dict<float,bool>":
                                        table[prop.Name] = HandleDict<float, bool>(prop.Value.ToObject<JObject>());
                                        break;
                                    case "dict<float,string>":
                                        table[prop.Name] = HandleDict<float, string>(prop.Value.ToObject<JObject>());
                                        break;
                                    case "dict<string,int>":
                                        table[prop.Name] = HandleDict<string, int>(prop.Value.ToObject<JObject>());
                                        break;
                                    case "dict<string,float>":
                                        table[prop.Name] = HandleDict<string, float>(prop.Value.ToObject<JObject>());
                                        break;
                                    case "dict<string,bool>":
                                        table[prop.Name] = HandleDict<string, bool>(prop.Value.ToObject<JObject>());
                                        break;
                                    case "dict<string,string>":
                                        table[prop.Name] = HandleDict<string, string>(prop.Value.ToObject<JObject>());
                                        break;
                                }
                            }

                            table.ForbidWrite();
                            _configDict[configName].Add(table);
                        }
                    }
                // }
                // catch (Exception ex)
                // {
                //     LogManager.Log(LOGTag, configName);
                //     LogManager.LogError(LOGTag, ex.ToString());
                // }
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
        private static dynamic HandleDict<TKey,TValue>(JObject jObj)
        {
            dynamic table = new RestrictedDictionary<TKey,TValue>();
            // LogManager.Log(typeof(TKey));
            table.EnableWrite();
            // var valueTypeDict = _typeDict[cfName];
            var properties = jObj.Properties();
            foreach (var prop in properties)
            {
                dynamic key = typeof(TKey).ToString() switch
                {
                    "System.Int32" => int.Parse(prop.Name),
                    "System.Single" => float.Parse(prop.Name),
                    "System.String" => prop.Name,
                    _ => default
                };
                dynamic value = prop.Value.Type.ToString() switch
                {
                    "Integer" => (int)prop.Value,
                    "Float" => (float)prop.Value,
                    "Boolean" => (bool)prop.Value,
                    "String" => prop.Value.ToString(),
                    _ => default
                };
                table.Add(key, value);
            }
            table.ForbidWrite();
            // LogManager.Log(LOGTag, table);    
            return table;
        }

        private static dynamic HandleArray1D<T>(IReadOnlyCollection<JToken> array)
        {
            // LogManager.Log(LOGTag,"array.Count",array.Count);
            dynamic table = new List<T>();
            foreach (var t in array)
            {
                switch (t.Type.ToString())
                {
                    case "Integer":
                        table.Add((int)t);
                        break;
                    case "Float":
                        table.Add((float)t);
                        break;
                    case "Boolean":
                        table.Add((bool)t);
                        break;
                    case "String":
                        table.Add(t.ToString());
                        break;
                }
            }

            return table;
        }
        private static dynamic HandleArray2D<T>(IEnumerable<JToken> array)
        {
            dynamic table = new List<List<T>>();
            foreach (var t in array)
            {
                var innerTable = new List<T>();
                for (var j = 0; j < t.ToArray().Length; j++)
                {
                    switch (t.Type.ToString())
                    {
                        case "Integer":
                            innerTable.Add(t.ToObject<T>());
                            break;
                        case "Float":
                            innerTable.Add(t.ToObject<T>());
                            break;
                        case "Boolean":
                            innerTable.Add(t.ToObject<T>());
                            break;
                        case "String":
                            innerTable.Add(t.ToObject<T>());
                            break;
                    }
                }
                table.Add(innerTable);
            }

            return table;
        }


        // 递归处理数组类型
        private static dynamic HandleArray<T>(IReadOnlyCollection<JToken> array,int deep = 1)
        {
            var table = deep switch
            {
                1 => HandleArray1D<T>(array),
                2 => HandleArray2D<T>(array),
                _ => HandleArray1D<T>(array)
            };
            return table;
        }

        /// <summary>
        /// 获取配置表
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static List<dynamic> GetConfig(EConfig configName)
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
        public static dynamic GetConfigByID(EConfig configName, int id)
        {
            try
            {
                if (ConfigNameDict.TryGetValue(configName, out var name))
                {
                    if (_configDict.TryGetValue(name, out var value))
                    {
                        //待优化直接取id的配置
                        foreach (var cf in value)
                        {
                            if (cf["id"] == id)
                            {
                                return cf;
                            }
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