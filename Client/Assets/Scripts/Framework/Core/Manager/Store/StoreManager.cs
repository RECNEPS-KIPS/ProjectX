// author:KIPKIPS
// describe:本地对象数据的存储和读取
using Framework.Core.Singleton;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace Framework.Core.Manager.Store {
    [MonoSingletonPath("[Manager]/StoreManager")]
    public class StoreManager : MonoSingleton<StoreManager> {
        private string logTag = "StoreManager";
        private string _storePath = System.Environment.CurrentDirectory + @"\Data\" + "LocalizeStoreData".GetHashCode();
        [System.Serializable]
        private class StorageContainer {
            public List<string> keyList = new List<string>();
            public List<dynamic> valueList = new List<dynamic>();
        }
        private StorageContainer _storageContainer = new StorageContainer();
        public void Launch() {
            InitStorageContainer();
        }

        // 初始化本地存储容器
        void InitStorageContainer() {
            if (File.Exists(_storePath)) {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream file = File.Open(_storePath, FileMode.Open);
                //反序列化
                _storageContainer = (StorageContainer)formatter.Deserialize(file);
                file.Close();
            }
        }
        //二进制写
        private void Write() {
            //二进制格式器
            BinaryFormatter formatter = new BinaryFormatter();
            //创建文件流来保存
            FileStream file = File.Create(_storePath);
            //开始序列化,第一个参数为路径,第二个参数为需要序列化的对象
            formatter.Serialize(file, _storageContainer);
            file.Close();
        }

        //二进制读
        private StorageContainer Read() {
            //二进制格式器
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(_storePath, FileMode.Open);
            //反序列化
            StorageContainer data = (StorageContainer)formatter.Deserialize(file);
            file.Close();
            return data;
        }

        //保存对象
        public void Save<T>(T obj) where T : ILocalizable {
            if (string.IsNullOrEmpty(obj.LocalizeKey)) {
                LogManager.LogError(logTag, $"The localize key '{obj.LocalizeKey}' cannot be an empty string");
                return;
            }
            if (_storageContainer.keyList.Contains(obj.LocalizeKey)) {
                _storageContainer.valueList[_storageContainer.keyList.IndexOf(obj.LocalizeKey)] = obj;
            } else {
                _storageContainer.keyList.Add(obj.LocalizeKey);
                _storageContainer.valueList.Add(obj);
            }
        }

        // 获取保存的对象
        public T TryGet<T>(string localizeKey, out T obj) where T : ILocalizable {
            if (string.IsNullOrEmpty(localizeKey)) {
                LogManager.LogWarning(logTag, $"The localize key '{localizeKey}' cannot be an empty string");
            }
            if (_storageContainer.keyList.Contains(localizeKey)) {
                obj = _storageContainer.valueList[_storageContainer.keyList.IndexOf(localizeKey)];
            } else {
                obj = default;
                LogManager.LogWarning(logTag, $"The object with key '{localizeKey}' does not exist");
            }
            return obj;
        }
    }
}