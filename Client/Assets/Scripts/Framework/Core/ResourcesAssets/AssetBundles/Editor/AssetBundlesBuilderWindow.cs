using System.Collections.Generic;
using System.IO;
using Framework.Core.Manager.ResourcesLoad;
using UnityEditor;
using UnityEngine;

namespace Framework.Core.ResourcesAssets
{
    public class AssetBundlesBuilderWindow : EditorWindow
    {
        private static string abOutPath;
        // private static List<AssetBundleBuild> ListAssets = new List<AssetBundleBuild>();
        private static readonly Dictionary<string,InnerAssetBundleBuild> AssetsDict = new ();

        private static bool IsFinished; //是否检查完成 可以打包

        private class InnerAssetBundleBuild
        {
            public string assetBundleName;
            public string assetBundleVariant;
            public List<string> assetNames;
        }
        private const string LOGTag = "AssetBundlesBuilder";
        
        private static AssetBundlesBuilderWindow window;
        private const int normalSpace = 20;

        private static float screenScale => Screen.dpi / DEF.SYSTEM_STANDARD_DPI;
        // private Rect _windowSize;
        private static Rect windowSize => new (0, 0, Screen.width / screenScale, Screen.height / screenScale);

        private static void CollectAllAssetBundlesData()
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
            
            const string assetMapPath = DEF.ASSET_BUNDLE_PATH;
            var assetMap = AssetDatabase.LoadAssetAtPath<AssetBundlesMap>(assetMapPath);
            if (assetMap) {
                assetMap.Map.Clear();
            }
            else
            {
                AssetsDict["Misc"].assetNames.Add(assetMapPath);
                assetMap = CreateInstance<AssetBundlesMap>();
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

            var arr = abList.ToArray();
            foreach (var abb in arr)
            {
                LogManager.Log(LOGTag,$"assetBundleName:{abb.assetBundleName}");
                foreach (var name in abb.assetNames)
                {
                    LogManager.Log(LOGTag,$"asset Name:{name}");
                }
            }
            return arr ;
        }

        [MenuItem("Tools/构建AssetsBundle面板", false,-1000)]
        public static void OpenBuildAssetBundlesWindow()
        {
            window = GetWindow<AssetBundlesBuilderWindow>("Build AssetsBundle Window");
            window.Show();
            // var position = window.position;
            window.minSize = new Vector2(560, 510);
            window.InitWindow();
        }
        
        private AssetBundlesBuildRule AssetBundlesBuildRule;
        private readonly Dictionary<string, AssetBundlesRule> RuleMap = new();
        private void InitWindow()
        {
            AssetBundlesBuildRule = ResourcesLoadManager.LoadAsset<AssetBundlesBuildRule>(DEF.ASSET_BUNDLE_RULE_PATH);
            if (AssetBundlesBuildRule != null)
            {
                foreach (var rule in AssetBundlesBuildRule.AssetBundlesRules)
                {
                    var vaild = (rule.IsDirectory && Directory.Exists(rule.FullPath)) || (!rule.IsDirectory && File.Exists(rule.FullPath));
                    if (vaild)
                    {
                        RuleMap.Add(rule.FullPath,rule);
                        SelectMap.Add(rule.FullPath,true);
                    }
                }
            }
            AssetsDict.Clear();
            abOutPath = AssetBundlesPathTools.GetABOutPath();
        }

        void Expand()
        {
            var mapKeys = RuleFlodMap.Keys;
            var keys = new string[mapKeys.Count];
            mapKeys.CopyTo(keys,0);
            foreach (var t in keys)
            {
                if (RuleFlodMap.ContainsKey(t))
                {
                    RuleFlodMap[t] = true;
                }
            }
        }
        private void BuildAssetBundles()
        {
            AssetsDict.Clear();
            RemoveABLabel();
            AssetBundlesBuildRule.AssetBundlesRules.Clear();
            foreach (var kvp in RuleMap)
            {
                AssetBundlesBuildRule.AssetBundlesRules.Add(kvp.Value);
            }

            EditorUtility.SetDirty(AssetBundlesBuildRule);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // LogManager.Log(LOGTag,"GetABOutPath",abOutPath);
            if (Directory.Exists(abOutPath))
            {
                Directory.Delete(abOutPath,true);
            }
            Directory.CreateDirectory(abOutPath);
            LogManager.Log(LOGTag,AssetBundlesPathTools.GetABResourcesPath(),abOutPath);
            SearchFileAssetBundleBuild();
            var assetBundleBuilds = GetAssetBundleBuilds();
            
            BuildPipeline.BuildAssetBundles(abOutPath,assetBundleBuilds , BuildAssetBundleOptions.None,EditorUserBuildSettings.activeBuildTarget);
            LogManager.Log(LOGTag,"AssetBundle打包完毕");
        }
        
        private readonly Dictionary<string, bool> RuleFlodMap = new();
        private readonly Dictionary<string, bool> SelectMap = new();
        private void DrawDir(string dirPath,string drawName,int layer)
        {
            if (!Directory.Exists(dirPath)) return;
            var dirInfo = new DirectoryInfo(dirPath);
            GUILayout.BeginHorizontal();

            GUILayout.Space(layer * tabSpace);
            RuleFlodMap.TryAdd(dirPath, layer <= 1);
            var foldHeader = drawName.Replace("\\", "/");
     
            RuleFlodMap[dirPath] = EditorGUILayout.BeginFoldoutHeaderGroup(RuleFlodMap[dirPath], foldHeader);
            EditorGUILayout.EndFoldoutHeaderGroup();
            var projectDirPath = $"Assets/{dirInfo.FullName.Replace("\\","/").Replace(Application.dataPath+"/","")}";
            // LogManager.Log(LOGTag,projectDirPath);
            GUILayout.FlexibleSpace();
            DrawRowRightPart(projectDirPath, dirInfo.Name,true);
            GUILayout.EndHorizontal();

            var dirs = dirInfo.GetDirectories();
            var files = dirInfo.GetFiles();
            if (dirs.Length > 0 || files.Length > 0)
            {
                layer++;
                if (RuleFlodMap[dirPath])
                {
                    foreach (var t in dirs)
                    {
                        DrawDir(t.FullName,t.Name,layer);
                    }

                    foreach (var t in files)
                    {
                        if (t.Name.EndsWith(".meta")) continue;

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(layer * tabSpace);
                        GUILayout.Label(t.Name.Replace("\\", "/"));
                        var projectFilePath = $"Assets/{t.FullName.Replace("\\","/").Replace(Application.dataPath+"/","")}";
                        GUILayout.FlexibleSpace();
                        DrawRowRightPart(projectFilePath, t.Name.Replace(t.Extension,""));
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }

        private void DrawRowRightPart(string fullProjectPath,string labelName,bool isDirectory = false)
        {
            GUILayout.Label("Build",GUILayout.Height(20),GUILayout.Width(35));
            SelectMap.TryAdd(fullProjectPath, false);
            SelectMap[fullProjectPath] = EditorGUILayout.Toggle(SelectMap[fullProjectPath],GUILayout.Height(20),GUILayout.Width(20));
            // var autoTagUI = false;
            var assetTypeUI = false;
            const int abLabelWidth = 55;
            const int abLabelTextFieldWidth = 150;
            const int assetTypeWidth = 65;
            const int assetTypeTextFieldWidth = 150;
            if (SelectMap[fullProjectPath])
            {
                if (!RuleMap.ContainsKey(fullProjectPath))
                {
                    RuleMap.Add(fullProjectPath,new AssetBundlesRule
                    {
                        FullPath = fullProjectPath,
                        ABLabel = labelName,
                        IsDirectory = isDirectory,
                        AssetType = "",
                    });
                }
                
                GUILayout.Label("ABLabel",GUILayout.Height(20),GUILayout.Width(abLabelWidth));
                RuleMap[fullProjectPath].ABLabel = EditorGUILayout.TextField(RuleMap[fullProjectPath].ABLabel,GUILayout.Height(20),GUILayout.Width(abLabelTextFieldWidth));
   
                if (RuleMap[fullProjectPath].IsDirectory)
                {
                    assetTypeUI = true;
                    GUIContent c = new GUIContent("AssetType", "asset type split with ';' exp: prefab;png");
                    GUILayout.Label(c,GUILayout.Height(20),GUILayout.Width(assetTypeWidth));
                    RuleMap[fullProjectPath].AssetType = EditorGUILayout.TextField(RuleMap[fullProjectPath].AssetType,GUILayout.Height(20),GUILayout.Width(assetTypeTextFieldWidth));
                }
                
                var totalSpace = 0;
                if (!assetTypeUI)
                {
                    totalSpace += assetTypeTextFieldWidth + assetTypeWidth + verticalScrollBar / 2;
                }
                GUILayout.Space(totalSpace);
            }
            else
            {
                if (RuleMap.ContainsKey(fullProjectPath))
                {
                    RuleMap.Remove(fullProjectPath);
                }
                GUILayout.Space(abLabelWidth + abLabelTextFieldWidth + assetTypeTextFieldWidth + assetTypeWidth + verticalScrollBar);
            }
        }

        private Vector2 sceneScrollPosition;
        private const int verticalScrollBar = 16;
        private const int tabSpace = 25;
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            sceneScrollPosition = GUILayout.BeginScrollView(sceneScrollPosition, false, true, GUILayout.Width(windowSize.width - 2), GUILayout.Height(windowSize.height - normalSpace * 6));
            DrawDir("Assets/ResourcesAssets","ResourcesAssets",0);
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Collapse All", GUILayout.Width(windowSize.width - 6), GUILayout.Height(normalSpace)))
            {
                var mapKeys = RuleFlodMap.Keys;
                var keys = new string[mapKeys.Count];
                mapKeys.CopyTo(keys,0);
                foreach (var t in keys)
                {
                    if (RuleFlodMap.ContainsKey(t))
                    {
                        RuleFlodMap[t] = false;
                    }
                }
            }
            if (GUILayout.Button("Expand All", GUILayout.Width(windowSize.width - 6), GUILayout.Height(normalSpace)))
            {
                Expand();
            }
            
            var c = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Build AssetBundle", GUILayout.Width(windowSize.width - 6), GUILayout.Height(normalSpace)))
            {
                BuildAssetBundles();
                GUIUtility.ExitGUI();
            }
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Remove All Label", GUILayout.Width(windowSize.width - 6), GUILayout.Height(normalSpace)))
            {
                RemoveABLabel();
                GUIUtility.ExitGUI();
            }
            GUI.backgroundColor = c;
            GUILayout.Space(3);
            GUILayout.EndVertical();
        }

        //是文件 继续向下
        public static void SearchFileAssetBundleBuild()
        {
            var ruleAsset = ResourcesLoadManager.LoadAsset<AssetBundlesBuildRule>(DEF.ASSET_BUNDLE_RULE_PATH);
            // LogManager.Log(LOGTag,ruleAsset == null);
            if (ruleAsset == null) return;
            // LogManager.Log(LOGTag,ruleAsset.AssetBundlesRules.Count);
            foreach (var rule in ruleAsset.AssetBundlesRules)
            {
                //文件夹拿取底下所有文件
                var tag = rule.ABLabel.ToLower();
                if (rule.IsDirectory)
                {
                    // rule.AssetType
                    var list =  Directory.EnumerateFiles(rule.FullPath, "*.*", SearchOption.AllDirectories);
                    if (rule.AssetType == string.Empty) continue;
                    var types = rule.AssetType.Split(";");
                    var typeDict = new HashSet<string>();
                    foreach (var type in types)
                    {
                        if (!string.IsNullOrEmpty(type) && !typeDict.Contains(type))
                        {
                            typeDict.Add($".{type}");
                        }
                    }
                    LogManager.Log(LOGTag,"types",types);
                    // LogManager.Log(LOGTag,list.Count());
                    foreach (var path in list)
                    {
                        if (path.EndsWith(".meta")) continue;
                        var fi = new FileInfo(path);
                        LogManager.Log(LOGTag,fi.Extension);
                        if (!typeDict.Contains(fi.Extension))continue;
                        
                        if (AssetsDict.TryGetValue(tag, out var value))
                        {
                            value.assetNames.Add(path);
                        }
                        else
                        {
                            AssetsDict.Add(tag,new InnerAssetBundleBuild
                            {
                                assetNames = new List<string> { path },
                                assetBundleName = tag,
                                assetBundleVariant = DEF.ASSET_BUNDLE_SUFFIX
                            });
                        }
                    }
                }
                else
                {
                    if (AssetsDict.TryGetValue(tag, out var value))
                    {
                        value.assetNames.Add(rule.FullPath);
                    }
                    else
                    {
                        AssetsDict.Add(tag,new InnerAssetBundleBuild
                        {
                            assetNames = new List<string> { rule.FullPath },
                            assetBundleName = tag,
                            assetBundleVariant = DEF.ASSET_BUNDLE_SUFFIX
                        });
                    }
                }
            }
        }
        
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
        private static void RemoveFileABLabel(FileSystemInfo fileInfoObj)
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