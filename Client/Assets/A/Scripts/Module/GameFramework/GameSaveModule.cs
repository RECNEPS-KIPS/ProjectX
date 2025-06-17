using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GameFramework;
using MemoryPack; // 添加MemoryPack引用

public class GameSaveModule : GameFrameworkModuleBase
{
    private string m_saveFilePath;
    private Dictionary<string, object> m_cacheData = new Dictionary<string, object>();
    private Dictionary<string,Func<object,byte[]>> m_saveDataFuncs = new Dictionary<string,Func<object,byte[]>>();
    private SavePlatformType m_currentPlatform;
    
    // 枚举不同平台存储类型
    private enum SavePlatformType
    {
        Local,      // PC和移动端本地存储
        CloudWeb,   // WebGL云存储
        None        // 未初始化
    }
    
    // 静态方法用于从编辑器中删除所有存档
    public static bool DeleteAllSaves()
    {
        try
        {
            // 删除PC/移动端本地存档
            string localSaveFilePath = Path.Combine(Application.persistentDataPath, "GameSave");
            if (Directory.Exists(localSaveFilePath))
            {
                var files = Directory.GetFiles(localSaveFilePath, "*.bin");
                foreach (var file in files)
                {
                    File.Delete(file);
                }
                Debug.Log("所有本地存档数据已删除");
            }
            
            // 如果有WebGL云存档，这里可以添加API调用
            // CloudSaveAPI.ClearAllData();
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"删除存档数据失败: {e.Message}");
            return false;
        }
    }
    
    public override void InitModule()
    {
        // 确定当前平台类型
        DeterminePlatformType();
        
        // 初始化存储路径
        InitSavePath();
        
        Debug.Log($"存档模块初始化完成，当前平台：{m_currentPlatform}，存档路径：{m_saveFilePath}");
    }
    
    public override void LoadModule()
    {
        // 加载缓存数据
        LoadCacheData();
    }
    
    public override void UpdateModule()
    {
        // 在这里可以添加定期自动保存或云同步的逻辑
    }
    
    public override void DisposeModule()
    {
        // 确保数据已保存
        SaveAllData();
        m_cacheData.Clear();
        Debug.Log("存档模块已释放");
    }
    
    private void DeterminePlatformType()
    {
        #if UNITY_WEBGL
            m_currentPlatform = SavePlatformType.CloudWeb;
        #else
            m_currentPlatform = SavePlatformType.Local;
        #endif
    }
    
    private void InitSavePath()
    {
        switch (m_currentPlatform)
        {
            case SavePlatformType.Local:
                m_saveFilePath = Path.Combine(Application.persistentDataPath, "GameSave");
                // 确保目录存在
                if (!Directory.Exists(m_saveFilePath))
                {
                    Directory.CreateDirectory(m_saveFilePath);
                }
                break;
                
            case SavePlatformType.CloudWeb:
                // WebGL平台的云存储路径，待接入第三方云存档
                m_saveFilePath = "CloudSave";
                Debug.Log("WebGL平台云存档功能待接入");
                break;
                
            default:
                Debug.LogError("未知的平台类型");
                break;
        }
    }
    
    // 保存指定键的数据
    public void SaveData<T>(string key, T data)
    {
        // 更新缓存
        m_cacheData[key] = data;
        // 根据平台执行不同的保存逻辑
        switch (m_currentPlatform)
        {
            case SavePlatformType.Local:
                SaveLocalData(key, data,m_saveDataFuncs[key]);
                break;
                
            case SavePlatformType.CloudWeb:
                SaveCloudData(key, data);
                break;
                
            default:
                Debug.LogError("未知的平台类型，无法保存数据");
                break;
        }
    }
    
    // 加载指定键的数据
    public T LoadSaveData<T>(string key,Func<T> createDefaultFunc = null)
    {
        // 先检查缓存
        if (m_cacheData.TryGetValue(key, out object cachedData) && cachedData is T typedData)
        {
            return typedData;
        }
        
        // 根据平台执行不同的加载逻辑
        T data = default;
        switch (m_currentPlatform)
        {
            case SavePlatformType.Local:
                data = LoadLocalSaveData<T>(key);
                break;
            case SavePlatformType.CloudWeb:
                data = LoadCloudSaveData<T>(key);
                break;
            default:
                Debug.LogError("未知的平台类型，无法加载数据");
                break;
        }
        // 保存方法
        if(!m_saveDataFuncs.ContainsKey(key))
        {
            m_saveDataFuncs[key] = (object data) => MemoryPackSerializer.Serialize((T)data);
        }
        // 成功加载后更新缓存
        if (data != null)
        {
            m_cacheData[key] = data;
        }
        else if(createDefaultFunc != null)
        {
            data = createDefaultFunc();
            m_cacheData[key] = data;
            SaveData(key, data);
        }
        return data;
    }
    
    // 保存所有缓存数据
    public void SaveAllData()
    {
        foreach (var kvp in m_cacheData)
        {
            // 根据平台类型调用对应方法
            switch (m_currentPlatform)
            {
                case SavePlatformType.Local:
                    SaveLocalData(kvp.Key, kvp.Value,m_saveDataFuncs[kvp.Key]);
                    break;
                    
                case SavePlatformType.CloudWeb:
                    SaveCloudData(kvp.Key, kvp.Value);
                    break;
                    
                default:
                    Debug.LogError("未知的平台类型，无法保存数据");
                    break;
            }
        }
        
        Debug.Log("所有数据已保存");
    }
    
    // 加载所有缓存数据
    private void LoadCacheData()
    {
        // 此处可实现读取存档目录下所有文件并加载到缓存的逻辑
        if (m_currentPlatform == SavePlatformType.Local && Directory.Exists(m_saveFilePath))
        {
            try
            {
                var files = Directory.GetFiles(m_saveFilePath, "*.bin");
                foreach (var file in files)
                {
                    string key = Path.GetFileNameWithoutExtension(file);
                    // 这里需要知道具体类型才能反序列化，可能需要额外的类型信息或使用泛型方法
                    // 这里仅作为框架，实际使用时需要根据游戏设计扩展
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"加载缓存数据失败: {e.Message}");
            }
        }
        Debug.Log("缓存数据加载完成");
    }
    
    #region 本地存储实现
    
    private void SaveLocalData<T>(string key, T data,Func<T,byte[]> serializeFunc)
    {
        try
        {
            string filePath = Path.Combine(m_saveFilePath, $"{key}.bin");
            // 使用MemoryPack序列化
            byte[] binaryData = serializeFunc != null ? serializeFunc(data) : MemoryPackSerializer.Serialize(data);
            File.WriteAllBytes(filePath, binaryData);
            Debug.Log($"本地数据已保存: {key}");
        }
        catch (Exception e)
        {
            Debug.LogError($"保存本地数据失败: {key}, 错误: {e.Message}");
        }
    }
    
    private T LoadLocalSaveData<T>(string key)
    {
        string filePath = Path.Combine(m_saveFilePath, $"{key}.bin");
        
        if (File.Exists(filePath))
        {
            try
            {
                // 使用MemoryPack反序列化
                byte[] binaryData = File.ReadAllBytes(filePath);
                Debug.Log($"本地数据已加载: {key}");
                var data = MemoryPackSerializer.Deserialize<T>(binaryData);
                if(data == null)
                {
                    Debug.LogError($"本地数据加载失败: {key}, 数据为空");
                    return default;
                }
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"加载本地数据失败: {key}, 错误: {e.Message}");
            }
        }
        return default;
    }
    
    #endregion
    
    #region 云存储实现
    
    private void SaveCloudData<T>(string key, T data)
    {
        // WebGL平台云存储实现
        // 这里需要接入第三方云存储服务
        Debug.Log($"云存储接口尚未实现，数据未保存: {key}");
        
        // 示例：当接入云存储API后的代码结构
        /*
        byte[] binaryData = MemoryPackSerializer.Serialize(data);
        string base64Data = Convert.ToBase64String(binaryData);
        CloudSaveAPI.Save(key, base64Data, success => {
            if (success) {
                Debug.Log($"云数据保存成功: {key}");
            } else {
                Debug.LogError($"云数据保存失败: {key}");
            }
        });
        */
    }
    
    private T LoadCloudSaveData<T>(string key)
    {
        // WebGL平台云存储实现
        // 这里需要接入第三方云存储服务
        Debug.Log($"云存储接口尚未实现，无法加载数据: {key}");
        return default;
        
        // 示例：当接入云存储API后的代码结构
        /*
        CloudSaveAPI.Load(key, (success, base64Data) => {
            if (success) {
                byte[] binaryData = Convert.FromBase64String(base64Data);
                data = MemoryPackSerializer.Deserialize<T>(binaryData);
                Debug.Log($"云数据加载成功: {key}");
                return true;
            } else {
                Debug.LogError($"云数据加载失败: {key}");
                data = default;
                return false;
            }
        });
        */
    }
    
    #endregion
    
    // 删除指定键的数据
    public bool DeleteData(string key)
    {
        // 从缓存中移除
        m_cacheData.Remove(key);
        
        switch (m_currentPlatform)
        {
            case SavePlatformType.Local:
                try
                {
                    string filePath = Path.Combine(m_saveFilePath, $"{key}.bin");
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        Debug.Log($"本地数据已删除: {key}");
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"删除本地数据失败: {key}, 错误: {e.Message}");
                }
                return false;
                
            case SavePlatformType.CloudWeb:
                // 云存储删除逻辑
                Debug.Log($"云存储接口尚未实现，无法删除数据: {key}");
                return false;
                
            default:
                Debug.LogError("未知的平台类型，无法删除数据");
                return false;
        }
    }
    
    // 清除所有存档数据
    public void ClearAllData()
    {
        // 清空缓存
        m_cacheData.Clear();
        
        switch (m_currentPlatform)
        {
            case SavePlatformType.Local:
                try
                {
                    // 删除存档目录下的所有文件
                    var files = Directory.GetFiles(m_saveFilePath, "*.bin");
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }
                    Debug.Log("所有本地存档数据已清除");
                }
                catch (Exception e)
                {
                    Debug.LogError($"清除本地存档数据失败: {e.Message}");
                }
                break;
                
            case SavePlatformType.CloudWeb:
                // 云存储清除逻辑
                Debug.Log("云存储接口尚未实现，无法清除所有数据");
                break;
                
            default:
                Debug.LogError("未知的平台类型，无法清除数据");
                break;
        }
    }
} 