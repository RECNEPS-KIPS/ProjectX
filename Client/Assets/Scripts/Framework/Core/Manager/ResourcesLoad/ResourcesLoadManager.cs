using System;
using System.Collections.Generic;
using Framework.Core.Singleton;
using UnityEngine;

namespace Framework.Core.Manager.ResourcesLoad {
    [MonoSingletonPath("[Manager]/ResourcesLoadManager")]
    public class ResourcesLoadManager : MonoSingleton<ResourcesLoadManager> {
        private const string logTag = "ResourcesLoadManager";
        private Dictionary<string, AssetBundle> m_AssetBundleDict = new Dictionary<string, AssetBundle>();
        private const string assetBundlePath = "AssetBundles";

        // 加载AssetBundle FromLocalFile
        public void LoadAssetBundleFile(string assetBundleName)
        {
            assetBundleName = assetBundleName.ToLower();
            var myLoadAssetBundle = AssetBundle.LoadFromFile($"{Application.dataPath}/{assetBundlePath}/{assetBundleName}");
            if (null == myLoadAssetBundle) {
                LogManager.LogError(logTag, "load AssetBundle == null");
                return;
            }
            m_AssetBundleDict[assetBundleName] = myLoadAssetBundle;
        }

        // AssetBundles本地加载
        public T LoadFromFile<T>(string assetBundleName, string assetName) where T : UnityEngine.Object 
        {
            T temp = default(T);
            assetBundleName = assetBundleName.ToLower();
            if (!m_AssetBundleDict.ContainsKey(assetBundleName))
            {
                LoadAssetBundleFile(assetBundleName);
            } 
            temp = m_AssetBundleDict[assetBundleName].LoadAsset<T>(assetName);
            if (null == temp) {
                LogManager.LogError(logTag, String.Format("load Asset fail ! name = {0}", assetName));
            }
            return temp;
        }
    }
}