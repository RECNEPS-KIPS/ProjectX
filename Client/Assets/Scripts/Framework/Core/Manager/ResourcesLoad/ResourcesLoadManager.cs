using System;
using System.Collections.Generic;
using Framework.Core.Singleton;
using UnityEngine;

namespace Framework.Core.Manager.ResourcesLoad {
    /// <summary>
    /// 资源加载管理器
    /// </summary>
    // [MonoSingletonPath("[Manager]/ResourcesLoadManager")]
    public class ResourcesLoadManager : Singleton<ResourcesLoadManager> {
        private const string LOGTag = "ResourcesLoadManager";
        private readonly Dictionary<string, AssetBundle> _assetBundleDict = new Dictionary<string, AssetBundle>();
        private const string AssetBundlePath = "AssetBundles";
        
        /// <summary>
        /// 加载AssetBundle FromLocalFile
        /// </summary>
        /// <param name="assetBundleName"></param>
        public void LoadAssetBundleFile(string assetBundleName)
        {
            assetBundleName = assetBundleName.ToLower();
            var myLoadAssetBundle = AssetBundle.LoadFromFile($"{Application.dataPath}/{AssetBundlePath}/{assetBundleName}");
            if (null == myLoadAssetBundle) {
                LogManager.LogError(LOGTag, "load AssetBundle == null");
                return;
            }
            _assetBundleDict[assetBundleName] = myLoadAssetBundle;
        }
        
        /// <summary>
        /// AssetBundles本地加载
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LoadFromFile<T>(string assetBundleName, string assetName) where T : UnityEngine.Object 
        {
            var temp = default(T);
            assetBundleName = assetBundleName.ToLower();
            if (!_assetBundleDict.ContainsKey(assetBundleName))
            {
                LoadAssetBundleFile(assetBundleName);
            } 
            temp = _assetBundleDict[assetBundleName].LoadAsset<T>(assetName);
            if (null == temp) {
                LogManager.LogError(LOGTag, $"load Asset fail ! name = {assetName}");
            }
            return temp;
        }
    }
}