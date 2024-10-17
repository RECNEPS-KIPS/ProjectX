using System;
using System.Collections.Generic;
using Framework.Core.ResourcesAssets;
using Framework.Core.Singleton;
using UnityEditor;
using UnityEngine;
using AssetBundle = UnityEngine.AssetBundle;

namespace Framework.Core.Manager.ResourcesLoad {
    /// <summary>
    /// 资源加载管理器
    /// </summary>
    // [MonoSingletonPath("[Manager]/ResourcesLoadManager")]
    public class ResourcesLoadManager : Singleton<ResourcesLoadManager> {
        private const string LOGTag = "ResourcesLoadManager";
        private readonly Dictionary<string, AssetBundle> _assetBundleDict = new Dictionary<string, AssetBundle>();
        private readonly Dictionary<string, string> _assetDict = new Dictionary<string, string>();

        public void Launch() 
        {
            var assetMapPath = DEF.ASSET_BUNDLE_PATH;
#if UNITY_EDITOR
            var assetMap = AssetDatabase.LoadAssetAtPath<AssetBundlesMap>(assetMapPath);
#else 
            var assetMap = LoadAsset<AssetBundlesMap>(DEF.ASSET_BUNDLE_PATH);
#endif
            
            foreach (var assetBundle in assetMap.Map) {
                foreach (var assetPath in assetBundle.assetBundlesMap) {
                    _assetDict.Add(assetPath,assetBundle.bundleName.ToLower());
                    LogManager.Log(LOGTag,assetPath,assetBundle.bundleName.ToLower());
                }
            }
        }
        /// <summary>
        /// 加载AssetBundle FromLocalFile
        /// </summary>
        /// <param name="assetBundleName"></param>
        public AssetBundle LoadAssetBundleFile(string assetBundleName) {
            if (_assetBundleDict.ContainsKey(assetBundleName)) {
                return _assetBundleDict[assetBundleName] ;
            }
            _assetBundleDict[assetBundleName] = AssetBundle.LoadFromFile($"{AssetBundlesPathTools.GetABOutPath()}/{assetBundleName}.{DEF.ASSET_BUNDLE_SUFFIX}");
            return _assetBundleDict[assetBundleName];
        }
        /// <summary>
        /// AssetBundles本地加载
        /// </summary>
        /// <param name="assetPath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LoadAsset<T>(string assetPath) where T : UnityEngine.Object {
            LogManager.Log(LOGTag,$"LoadAsset->assetPath:{assetPath}");
            var temp = default(T);
#if UNITY_EDITOR
            temp = AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
            if (_assetDict.ContainsKey(assetPath)) {
                AssetBundle ab = LoadAssetBundleFile(_assetDict[assetPath]);
                temp = ab.LoadAsset<T>(assetPath);
            } else {
                LogManager.Log(LOGTag,"资源不在assetbundle中");//Assets/ResourcesAssets/UI/Start/StartWindow.prefab
            }
            
#endif
            return temp;
        }
    }
}