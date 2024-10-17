using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework.Core.ResourcesAssets
{
    public class CreateAB : MonoBehaviour
    {
        private static string abOutPath;
        private static List<AssetBundleBuild> listassets = new List<AssetBundleBuild>();
        private static List<DirectoryInfo> listfileinfo = new List<DirectoryInfo>();
        private static bool isover = false; //是否检查完成，可以打包
        static private string selectPath;

        public static bool GetState()
        {
            return isover;
        }

        public static AssetBundleBuild[] GetAssetBundleBuilds()
        {
            return listassets.ToArray();
        }

        [MenuItem("ABTools/CreatAB &_Q", false)]
        public static void CreateModelAB()
        {
            abOutPath = Application.streamingAssetsPath;

            if (!Directory.Exists(abOutPath))
                Directory.CreateDirectory(abOutPath);

            UnityEngine.Object obj = Selection.activeObject;
            selectPath = AssetDatabase.GetAssetPath(obj);
            SearchFileAssetBundleBuild(selectPath);

            BuildPipeline.BuildAssetBundles(abOutPath,
                CreateAB.GetAssetBundleBuilds(), BuildAssetBundleOptions.None,
                EditorUserBuildSettings.activeBuildTarget);
            Debug.Log("AssetBundle打包完毕");
        }

        [MenuItem("ABTools/CreatAB &_Q", true)]
        public static bool CanCreatAB()
        {
            if (Selection.objects.Length > 0)
            {
                return true;
            }
            else
                return false;
        }

//是文件，继续向下
        public static void SearchFileAssetBundleBuild(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(@path);
            FileSystemInfo[] fileSystemInfos = directory.GetFileSystemInfos();
            listfileinfo.Clear();
            //遍历所有文件夹中所有文件
            foreach (var item in fileSystemInfos)
            {
                int idx = item.ToString().LastIndexOf(@"\");
                string name = item.ToString().Substring(idx + 1);
                //item为文件夹，添加进listfileinfo，递归调用
                if ((item as DirectoryInfo) != null)
                    listfileinfo.Add(item as DirectoryInfo);

                //剔除meta文件，其他文件都创建AssetBundleBuild,添加进listassets；
                if (!name.Contains(".meta"))
                {
                    CheckFileOrDirectoryReturnBundleName(item, path + "/" + name);
                }
            }

            if (listfileinfo.Count == 0)
                isover = true;
            else
            {
                Debug.LogError(listfileinfo.Count);
            }
        }

        //判断是文件还是文件夹
        public static string CheckFileOrDirectoryReturnBundleName(FileSystemInfo fileSystemInfo, string path)
        {
            FileInfo fileInfo = fileSystemInfo as FileInfo;
            if (fileInfo != null)
            {
                string[] strs = path.Split('.');
                string[] dictors = strs[0].Split('/');
                string name = "";
                for (int i = 1; i < dictors.Length; i++)
                {
                    if (i < dictors.Length - 1)
                    {
                        name += dictors[i] + "/";
                    }
                    else
                    {
                        name += dictors[i];
                    }
                }

                string[] strName = selectPath.Split('/');
                AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
                assetBundleBuild.assetBundleName = strName[strName.Length - 1];
                assetBundleBuild.assetBundleVariant = "ab";
                assetBundleBuild.assetNames = new string[] { path };
                listassets.Add(assetBundleBuild);
                return name;
            }
            else
            {
                //递归调用
                SearchFileAssetBundleBuild(path);
                return null;
            }
        }
    }
}