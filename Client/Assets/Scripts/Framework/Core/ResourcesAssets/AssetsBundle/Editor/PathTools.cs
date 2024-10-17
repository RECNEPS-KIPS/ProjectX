using UnityEngine;

namespace Framework.Core.ResourcesAssets
{
    public class PathTools
    {
        // 打包AB包根路径
        public const string AB_RESOURCES = "ResourcesAssets"; 
        
        // 得到 AB 资源的输入目录
        public static string GetABResourcesPath()
        {
            return Application.dataPath + "/" + AB_RESOURCES;
        }

        // 获得 AB 包输出路径
        public static string GetABOutPath()
        {
            return GetPlatformPath() + "/" + GetPlatformName();
        }
        
        //获得平台路径
        private static string GetPlatformPath()
        {
            string strReturenPlatformPath = string.Empty;

    #if UNITY_STANDALONE_WIN
            strReturenPlatformPath = Application.streamingAssetsPath;
    #elif UNITY_IPHONE
                strReturenPlatformPath = Application.persistentDataPath;
    #elif UNITY_ANDROID
                strReturenPlatformPath = Application.persistentDataPath;
    #endif
            
            return strReturenPlatformPath;
        }
        
        // 获得平台名称
        public static string GetPlatformName()
        {
            string strReturenPlatformName = string.Empty;

    #if UNITY_STANDALONE_WIN
            strReturenPlatformName = "Windows";
    #elif UNITY_IPHONE
                strReturenPlatformName = "IPhone";
    #elif UNITY_ANDROID
                strReturenPlatformName = "Android";
    #endif

            return strReturenPlatformName;
        }
        
        // 返回 WWW 下载 AB 包加载路径
        public static string GetWWWAssetBundlePath()
        {
            string strReturnWWWPath = string.Empty;

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
