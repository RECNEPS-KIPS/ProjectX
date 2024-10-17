using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework.Core.ResourcesAssets
{
    public class CreateAB : MonoBehaviour
    {
        private static string abOutPath;
        private static List<AssetBundleBuild> ListAssets = new List<AssetBundleBuild>();
        private static List<DirectoryInfo> ListFileInfo = new List<DirectoryInfo>();
        private static bool IsFinished; //是否检查完成 可以打包
        private static string selectPath;

        private const string LOGTag = "ResourcesAssets CreateAB";

        public static bool GetState()
        {
            return IsFinished;
        }

        public static AssetBundleBuild[] GetAssetBundleBuilds()
        {
            foreach (var AssetBundleBuild in ListAssets.ToArray())
            {
                foreach (var name in AssetBundleBuild.assetNames)
                {
                    LogManager.Log(LOGTag,$"AssetBundleBuild BundleName:{AssetBundleBuild.assetBundleName} - Path:{name}");
                }
            }
            return ListAssets.ToArray();
        }

        [MenuItem("ABTools/CreatAB &_Q", false)]
        public static void CreateModelAB()
        {
            ListAssets.Clear();
            abOutPath = PathTools.GetABOutPath();
            // LogManager.Log(LOGTag,"GetABOutPath",abOutPath);
            if (Directory.Exists(abOutPath))
            {
                Directory.Delete(abOutPath,true);
            }
            Directory.CreateDirectory(abOutPath);

            var obj = Selection.activeObject;
            selectPath = AssetDatabase.GetAssetPath(obj);
            // LogManager.Log(LOGTag,selectPath,abOutPath);
            
            SearchFileAssetBundleBuild(selectPath);
            var assetBundleBuilds = GetAssetBundleBuilds();
            
            // BuildPipeline.BuildAssetBundles(abOutPath,assetBundleBuilds , BuildAssetBundleOptions.None,EditorUserBuildSettings.activeBuildTarget);
            LogManager.Log(LOGTag,"AssetBundle打包完毕");
        }

        [MenuItem("ABTools/CreatAB &_Q", true)]
        public static bool CanCreatAB()
        {
            // LogManager.Log("CanCreatAB",Selection.objects.Length);
            return Selection.objects.Length > 0;
        }

        //是文件 继续向下
        public static void SearchFileAssetBundleBuild(string path)
        {
            var directory = new DirectoryInfo(path);
            // LogManager.Log("SearchFileAssetBundleBuild",Application.dataPath,path,directory == null);
            var fileSystemInfos = directory.GetFileSystemInfos();
            ListFileInfo.Clear();
            // LogManager.Log(LOGTag,"fileSystemInfos Length:",fileSystemInfos.Length);
            //遍历所有文件夹中所有文件
            foreach (var item in fileSystemInfos)
            {
                var str = item.ToString();
                var idx = str.LastIndexOf(@"\");
                var name = str.Substring(idx + 1);
                //item为文件夹 添加进ListFileInfo 递归调用
                if (item is DirectoryInfo info)
                {
                    // LogManager.Log(LOGTag,"SearchFileAssetBundleBuild Info:",info.FullName);
                    ListFileInfo.Add(info);
                }

                //剔除meta文件 其他文件都创建AssetBundleBuild,添加进ListAssets；
                if (!name.Contains(".meta"))
                {
                    CheckFileOrDirectoryReturnBundleName(item, path + "/" + name);
                }
            }

            if (ListFileInfo.Count == 0)
            {
                IsFinished = true;
            }
            else
            {
                LogManager.LogError(ListFileInfo.Count);
            }
        }

        //判断是文件还是文件夹
        public static void CheckFileOrDirectoryReturnBundleName(FileSystemInfo fileSystemInfo, string path)
        {
            if (fileSystemInfo is FileInfo)
            {
                // var strs = path.Split('.');
                // var dictors = strs[0].Split('/');
                // var name = "";
                // for (var i = 1; i < dictors.Length; i++)
                // {
                //     if (i < dictors.Length - 1)
                //     {
                //         name += dictors[i] + "/";
                //     }
                //     else
                //     {
                //         name += dictors[i];
                //     }
                // }
                var t= path.Replace("Assets/ResourcesAssets/", "");
                var tag = t.Split("/")[0];
                AssetImporter tmpImportObj = AssetImporter.GetAtPath(path);
                tmpImportObj.assetBundleName = tag;
                // var strName = selectPath.Split('/');
                var assetBundleBuild = new AssetBundleBuild
                {
                    assetBundleName = tag,//strName[strName.Length - 1],
                    assetBundleVariant = "ab",
                    assetNames = new[] { path }
                };
                ListAssets.Add(assetBundleBuild);
                // return name;
            }
            else
            {
                //递归调用
                SearchFileAssetBundleBuild(path);
                // return null;
            }
        }
    }
}