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
        private static readonly Dictionary<string, AssetBundle> _assetBundleDict = new Dictionary<string, AssetBundle>();
        private static readonly Dictionary<string, string> _assetDict = new Dictionary<string, string>();

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
        /// <param name="isAsync"></param>
        public static AssetBundle LoadAssetBundleFile(string assetBundleName,bool isAsync = false) {
            if (_assetBundleDict.TryGetValue(assetBundleName, out var file)) {
                LogManager.Log(LOGTag,$"LoadAssetBundleFile assetBundleName has loaded");
                return file;
            }
            // if (isAsync) {
            //     AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync($"{AssetBundlesPathTools.GetABOutPath()}/{assetBundleName}.{DEF.ASSET_BUNDLE_SUFFIX}");
            //     // abcr.completed +=  asyncOperation => {
            //     //     _assetBundleDict[assetBundleName] = abcr.assetBundle;
            //     // };
            // } else {
            // }
            _assetBundleDict[assetBundleName] = AssetBundle.LoadFromFile($"{AssetBundlesPathTools.GetABOutPath()}/{assetBundleName}.{DEF.ASSET_BUNDLE_SUFFIX}");
            LogManager.Log(LOGTag,$"LoadAssetBundleFile assetBundleName has loaded first");
            return _assetBundleDict[assetBundleName];
        }
        
        public static string GetAssetBundleName(string assetPath) {
            if (_assetDict.TryGetValue(assetPath, out var name))
            {
                LogManager.Log(LOGTag,$"资源{assetPath} is in {name}.ab");
                return name;
            }
            LogManager.Log(LOGTag,"资源不在assetbundle中");
            return string.Empty;
        }

        /// <summary>
        /// AssetBundles本地加载
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="isAsync"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T LoadAsset<T>(string assetPath,bool isAsync = false) where T : UnityEngine.Object {
            LogManager.Log(LOGTag,$"LoadAsset->assetPath:{assetPath}");
            T temp = default;
#if UNITY_EDITOR
            temp = AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
            if (_assetDict.ContainsKey(assetPath)) {
                AssetBundle ab = LoadAssetBundleFile(_assetDict[assetPath],isAsync);
                temp = ab.LoadAsset<T>(assetPath);
            } else {
                LogManager.Log(LOGTag,"资源不在assetbundle中");//Assets/ResourcesAssets/UI/Start/StartWindow.prefab
            }
            
#endif
            return temp;
        }
    }
}