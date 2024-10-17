// author:KIPKIPS
// describe:本地对象数据的存储和读取

using Framework.Core.Singleton;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace Framework.Core.Manager.Store
{
    /// <summary>
    /// 本地存储管理器
    /// </summary>
    // [MonoSingletonPath("[Manager]/StoreManager")]
    public class StoreManager : Singleton<StoreManager>
    {
        private const string LOGTag = "StoreManager";

        private readonly string _storePath =
            System.Environment.CurrentDirectory + @"\Data\" + "StoreData".GetHashCode();

        [System.Serializable]
        private class StorageContainer
        {
            public List<string> keyList = new List<string>();
            public List<dynamic> ValueList = new List<dynamic>();
        }

        private StorageContainer _storageContainer = new StorageContainer();

        /// <summary>
        /// 启动函数
        /// </summary>
        public void Launch()
        {
            InitStorageContainer();
        }

        public StoreManager()
        {
        }

        // 初始化本地存储容器
        private void InitStorageContainer()
        {
            if (!File.Exists(_storePath)) return;
            var formatter = new BinaryFormatter();
            var file = File.Open(_storePath, FileMode.Open);
            //反序列化
            _storageContainer = (StorageContainer)formatter.Deserialize(file);
            file.Close();
        }

        //二进制写
        private void Write()
        {
            //二进制格式器
            var formatter = new BinaryFormatter();
            //创建文件流来保存
            var file = File.Create(_storePath);
            //开始序列化,第一个参数为路径,第二个参数为需要序列化的对象
            formatter.Serialize(file, _storageContainer);
            file.Close();
        }

        //二进制读
        private StorageContainer Read()
        {
            //二进制格式器
            var formatter = new BinaryFormatter();
            var file = File.Open(_storePath, FileMode.Open);
            //反序列化
            var data = (StorageContainer)formatter.Deserialize(file);
            file.Close();
            return data;
        }


        /// <summary>
        /// 保存对象
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        public void Save<T>(T obj) where T : IStorable
        {
            if (string.IsNullOrEmpty(obj.StoreKey))
            {
                LogManager.LogError(LOGTag, $"The localize key '{obj.StoreKey}' cannot be an empty string");
                return;
            }

            if (_storageContainer.keyList.Contains(obj.StoreKey))
            {
                _storageContainer.ValueList[_storageContainer.keyList.IndexOf(obj.StoreKey)] = obj;
            }
            else
            {
                _storageContainer.keyList.Add(obj.StoreKey);
                _storageContainer.ValueList.Add(obj);
            }
        }

        /// <summary>
        /// 获取保存的对象
        /// </summary>
        /// <param name="localizeKey"></param>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T TryGet<T>(string localizeKey, out T obj) where T : IStorable
        {
            if (string.IsNullOrEmpty(localizeKey))
            {
                LogManager.LogWarning(LOGTag, $"The localize key '{localizeKey}' cannot be an empty string");
            }

            if (_storageContainer.keyList.Contains(localizeKey))
            {
                obj = _storageContainer.ValueList[_storageContainer.keyList.IndexOf(localizeKey)];
            }
            else
            {
                obj = default;
                LogManager.LogWarning(LOGTag, $"The object with key '{localizeKey}' does not exist");
            }

            return obj;
        }
    }
}