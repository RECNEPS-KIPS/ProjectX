using System;
using UnityEngine;

namespace Framework.Core.ResourcesAssets
{
    public class AssetBundlesPathTools
    {
        // 打包AB包根路径
        public const string AB_RESOURCES = "ResourcesAssets";

        private const string LOGTag = "AssetBundlesPathTools";
        // 得到 AB 资源的输入目录
        public static string GetABResourcesPath()
        {
            var resourcesPath = Application.dataPath + "/" + AB_RESOURCES;
            // LogManager.Log(LOGTag,"GetABResourcesPath:",resourcesPath);
            return resourcesPath;
        }

        // 获得 AB 包输出路径
        public static string GetABOutPath()
        {
            var outPath = GetPlatformPath() + "/" + GetPlatformName();
            // LogManager.Log(LOGTag,"GetABOutPath:",outPath);
            return outPath;
        }

        //获得平台路径
        private static string GetPlatformPath()
        {
            var strReturenPlatformPath = string.Empty;
#if UNITY_STANDALONE_WIN
            strReturenPlatformPath = Application.streamingAssetsPath;
#elif UNITY_IPHONE
            strReturenPlatformPath = Application.persistentDataPath;
#elif UNITY_ANDROID
            strReturenPlatformPath = Application.persistentDataPath;
#endif
            // LogManager.Log(LOGTag,"GetPlatformPath:",strReturenPlatformPath);
            return strReturenPlatformPath;
        }

        // 获得平台名称
        public static string GetPlatformName()
        {
            var strReturenPlatformName = string.Empty;
#if UNITY_STANDALONE_WIN
            strReturenPlatformName = "Windows";
#elif UNITY_IPHONE
            strReturenPlatformName = "IPhone";
#elif UNITY_ANDROID
            strReturenPlatformName = "Android";
#endif
            // LogManager.Log(LOGTag,"GetPlatformName:",strReturenPlatformName);
            return strReturenPlatformName;
        }

        // 返回 WWW 下载 AB 包加载路径
        public static string GetWWWAssetBundlePath()
        {
            var strReturnWWWPath = string.Empty;
#if UNITY_STANDALONE_WIN
            strReturnWWWPath = "file://" + GetABOutPath();
#elif UNITY_IPHONE
            strReturnWWWPath = GetABOutPath() + "/Raw/";
#elif UNITY_ANDROID
            strReturnWWWPath = "jar:file://" + GetABOutPath();
#endif
            return strReturnWWWPath;
        }
    }
}