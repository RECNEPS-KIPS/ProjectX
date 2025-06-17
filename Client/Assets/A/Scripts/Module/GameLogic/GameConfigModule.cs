using System;
using System.Collections.Generic;
using System.IO;
using MemoryPack;
using UnityEngine;
using GameFramework;

namespace GameLogic
{
    /// <summary>
    /// 游戏配置模块 - 负责加载和管理游戏配置数据
    /// </summary>
    public partial  class GameConfigModule : GameLogicModuleBase
    {
        // 保存已加载的配置数据的字典
        private Dictionary<Type, object> m_configDatas = new Dictionary<Type, object>();
        // 配置文件的存储路径
        private const string CONFIG_DATA_PATH = "ConfigData/";
        
        public override void InitModule()
        {
            m_configDatas.Clear();
        }
        
        public override void LoadModule()
        {
            
        }
        
        public override void DisposeModule()
        {
            // 清理配置数据
            m_configDatas.Clear();
        }
        
        
        /// <summary>
        /// 加载指定类型的配置数据(从自定义路径)
        /// </summary>
        /// <typeparam name="T">配置数据类型</typeparam>
        /// <param name="customPath">自定义路径</param>
        /// <returns>配置数据列表</returns>
        public List<T> LoadConfigFromPath<T>(string customPath) where T : class
        {
            var gameResourceLoadModule = GameRoot.Instance.GetGameModule<GameResourceLoadModule>();
            try
            {
                // 加载配置文件的字节数据
                TextAsset textAsset = gameResourceLoadModule.LoadResource<TextAsset>(customPath);
                if (textAsset == null)
                {
                    Debug.LogError($"配置文件不存在: {customPath}");
                    return null;
                }
                
                // 使用MemoryPack反序列化配置数据
                byte[] bytes = textAsset.bytes;
                List<T> configData = MemoryPackSerializer.Deserialize<List<T>>(bytes);
                m_configDatas[typeof(T)] = configData;
                return configData;
            }
            catch (Exception e)
            {
                Debug.LogError($"加载配置文件失败: {customPath}, 错误: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 获取指定类型的配置数据(如果已加载)
        /// </summary>
        /// <typeparam name="T">配置数据类型</typeparam>
        /// <returns>配置数据列表，未加载时返回null</returns>
        public List<T> GetConfig<T>() where T : class
        {
            Type configType = typeof(T);
            if (m_configDatas.TryGetValue(configType, out object cachedConfig))
            {
                return cachedConfig as List<T>;
            }
            
            Debug.LogError($"配置文件未找到: {typeof(T).Name}");
            return null;
        }
        
        /// <summary>
        /// 清除指定类型的配置缓存
        /// </summary>
        /// <typeparam name="T">配置数据类型</typeparam>
        public void ClearConfig<T>() where T : class
        {
            Type configType = typeof(T);
            if (m_configDatas.ContainsKey(configType))
            {
                m_configDatas.Remove(configType);
            }
        }
        
        /// <summary>
        /// 清除所有配置缓存
        /// </summary>
        public void ClearAllConfigs()
        {
            m_configDatas.Clear();
        }
    }
} 