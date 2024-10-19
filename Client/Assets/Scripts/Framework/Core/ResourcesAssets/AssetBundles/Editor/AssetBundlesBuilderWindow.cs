using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework.Core.ResourcesAssets
{
    public class AssetBundlesBuilderWindow : MonoBehaviour
    {
        private static string abOutPath;
        // private static List<AssetBundleBuild> ListAssets = new List<AssetBundleBuild>();
        private static Dictionary<string,InnerAssetBundleBuild> AssetsDict = new Dictionary<string,InnerAssetBundleBuild>();
        private static List<DirectoryInfo> ListFileInfo = new List<DirectoryInfo>();
        private static bool IsFinished; //是否检查完成 可以打包

        class InnerAssetBundleBuild
        {
            public string assetBundleName;
            public string assetBundleVariant;
            public List<string> assetNames;
        }
        private const string LOGTag = "AssetBundlesBuilder";

        private static readonly HashSet<string> BundleFileTypeMap = new()
        {
            "unity","prefab","asset"
        };

        public static bool GetState()
        {
            return IsFinished;
        }

        static void CollectAllAssetBundlesData()
        {
            LogManager.Log(LOGTag,"CollectAllAssetBundlesData");
            //先把资源映射表的引用添加进来
            if (!AssetsDict.ContainsKey("Misc"))
            {
                AssetsDict.Add("Misc",new InnerAssetBundleBuild());
                AssetsDict["Misc"].assetNames = new List<string>();
                AssetsDict["Misc"].assetBundleVariant = "ab";
                AssetsDict["Misc"].assetBundleName = "Misc";
            }
            
            var assetMapPath = DEF.ASSET_BUNDLE_PATH;
            var assetMap = AssetDatabase.LoadAssetAtPath<AssetBundlesMap>(assetMapPath);
            if (assetMap) {
                assetMap.Map.Clear();
            }
            else
            {
                AssetsDict["Misc"].assetNames.Add(assetMapPath);
                assetMap = ScriptableObject.CreateInstance<AssetBundlesMap>();
                AssetDatabase.CreateAsset(assetMap,assetMapPath);
            }
            var tmpImportObj = AssetImporter.GetAtPath(assetMapPath);
            tmpImportObj.assetBundleName = "Misc";
            
            foreach (var kvp in AssetsDict)
            {
                assetMap.Map.Add(new AssetBundle(kvp.Key,kvp.Value.assetNames));
            }
            EditorUtility.SetDirty(assetMap);
            AssetDatabase.SaveAssetIfDirty(assetMap);
            AssetDatabase.Refresh();
        }

        public static AssetBundleBuild[] GetAssetBundleBuilds()
        {
            CollectAllAssetBundlesData();
            var abList = new List<AssetBundleBuild>();
            foreach (var kvp in AssetsDict)
            {
                abList.Add(new AssetBundleBuild
                {
                    assetBundleName = kvp.Value.assetBundleName,
                    assetBundleVariant = kvp.Value.assetBundleVariant,
                    assetNames = kvp.Value.assetNames.ToArray()
                });
            }
            return abList.ToArray();
        }

        [MenuItem("Tools/ResourcesAssets/Build AssetsBundle", false)]
        public static void BuildAssetsBundle()
        {
            AssetsDict.Clear();
            abOutPath = AssetBundlesPathTools.GetABOutPath();
            // LogManager.Log(LOGTag,"GetABOutPath",abOutPath);
            if (Directory.Exists(abOutPath))
            {
                Directory.Delete(abOutPath,true);
            }
            Directory.CreateDirectory(abOutPath);


            LogManager.Log(LOGTag,AssetBundlesPathTools.GetABResourcesPath(),abOutPath);
            
            SearchFileAssetBundleBuild(AssetBundlesPathTools.GetABResourcesPath());
            var assetBundleBuilds = GetAssetBundleBuilds();
   
            BuildPipeline.BuildAssetBundles(abOutPath,assetBundleBuilds , BuildAssetBundleOptions.None,EditorUserBuildSettings.activeBuildTarget);
            LogManager.Log(LOGTag,"AssetBundle打包完毕");
            // var assetMapPath = DEF.ASSET_BUNDLE_PATH;
            // var assetMap = AssetDatabase.LoadAssetAtPath<AssetBundlesMap>(assetMapPath);
            // LogManager.Log(LOGTag,assetMap.Map.Count);
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
                var idx = str.LastIndexOf(@"\", StringComparison.Ordinal);
                var name = str[(idx + 1)..];
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
                // LogManager.LogError(ListFileInfo.Count);
            }
        }

        public static bool CheckBuildAssetBundle(string str)
        {
            if (str.EndsWith(".meta"))
            {
                return false;
            }
            var idx = str.LastIndexOf(@".", StringComparison.Ordinal);
            var type = str[(idx + 1)..];
            return BundleFileTypeMap.Contains(type);
        }

        //判断是文件还是文件夹
        public static void CheckFileOrDirectoryReturnBundleName(FileSystemInfo fileSystemInfo, string path)
        {
            if (fileSystemInfo is FileInfo)
            {
                path = $"Assets/{path.Replace($"{Application.dataPath}/", "")}";
                var tag = path.Replace($"Assets/{AssetBundlesPathTools.AB_RESOURCES}/","").Split("/")[0];
                var tmpImportObj = AssetImporter.GetAtPath(path);
                tmpImportObj.assetBundleName = tag;
                
                var Build = CheckBuildAssetBundle(path);
                LogManager.Log(LOGTag,$"Check File{path} {Build}");
                if (!Build) return;
                LogManager.Log(LOGTag,$"path:{path},tag:{tag}");
                if (AssetsDict.ContainsKey(tag))
                {
                    AssetsDict[tag].assetNames.Add(path);
                }
                else
                {
                    AssetsDict.Add(tag,new InnerAssetBundleBuild());
                    AssetsDict[tag].assetNames = new List<string> { path };
                    AssetsDict[tag].assetBundleName = tag;
                    AssetsDict[tag].assetBundleVariant = DEF.ASSET_BUNDLE_SUFFIX;
                }

            }
            else
            {
                SearchFileAssetBundleBuild(path);
            }
        }
        
        
        [MenuItem("Tools/ResourcesAssets/Remove AB Label")]
        public static void RemoveABLabel()
        {
            // 需要移除标记的根目录
            // 目录信息（场景目录信息数组 表示所有根目录下场景目录）

            // 定义需要移除AB标签的资源的文件夹根目录
            var strNeedRemoveLabelRoot = AssetBundlesPathTools.GetABResourcesPath();

            var dirTempInfo = new DirectoryInfo(strNeedRemoveLabelRoot);
            var directoryDIRArray = dirTempInfo.GetDirectories();

            // 遍历本场景目录下所有的目录或者文件
            foreach (var currentDir in directoryDIRArray)
            {
                // 递归调用方法 找到文件 则使用 AssetImporter 类 标记“包名”与 “后缀名”
                JudgeDirOrFileByRecursive(currentDir);
            }
            
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();

            // 提示信息 标记包名完成
            LogManager.Log("AssetBundle 本次操作移除标记完成");
        }

        /// <summary>
        /// 递归判断判断是否是目录或文件
        /// 是文件 修改 Asset Bundle 标记
        /// 是目录 则继续递归
        /// </summary>
        /// <param name="fileSystemInfo">当前文件信息（文件信息与目录信息可以相互转换）</param>
        private static void JudgeDirOrFileByRecursive(FileSystemInfo fileSystemInfo)
        {
            // 参数检查
            if (fileSystemInfo.Exists == false)
            {
                LogManager.LogError("文件或者目录名称：" + fileSystemInfo + " 不存在 请检查");
                return;
            }

            // 得到当前目录下一级的文件信息集合
            var directoryInfoObj = fileSystemInfo as DirectoryInfo;
            // 文件信息转为目录信息
            var fileSystemInfoArray = directoryInfoObj?.GetFileSystemInfos();

            if (fileSystemInfoArray == null) return;
            foreach (var fileInfo in fileSystemInfoArray)
            {
                // 文件类型
                if (fileInfo is FileInfo fileInfoObj)
                {
                    // 修改此文件的 AssetBundle 标签
                    RemoveFileABLabel(fileInfoObj);
                }
                // 目录类型
                else
                {
                    // 如果是目录 则递归调用
                    JudgeDirOrFileByRecursive(fileInfo);
                }
            }
        }

        /// <summary>
        /// 给文件移除 Asset Bundle 标记
        /// </summary>
        /// <param name="fileInfoObj">文件（文件信息）</param>
        static void RemoveFileABLabel(FileInfo fileInfoObj)
        {
            // AssetBundle 包名称
            // 参数检查（*.meta 文件不做处理）
            if (fileInfoObj.Extension == ".meta")
            {
                return;
            }
            
            // 获取资源文件的相对路径
            var strAssetFilePath = $"Assets{fileInfoObj.FullName.Replace("\\","/").Replace(Application.dataPath,"")}";
            // LogManager.Log(strAssetFilePath);
            // 给资源文件移除 AB 名称
            var tmpImportObj = AssetImporter.GetAtPath(strAssetFilePath);
            tmpImportObj.assetBundleName = string.Empty;
        }
    }
}