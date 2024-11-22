using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Framework.Common
{
    public class AssetHelperWindow : EditorWindow
    {
        public static string assetSrcFolderPath;
        public static string assetDstFolderPath;
        public static bool UseSourcePath;
        private const string LOGTag = "AssetHelper";
        [MenuItem("Tools/资源处理/提取FBX AnimationClip")]
        public static void ShowWindow()
        {
            var thisWindow = GetWindow(typeof(AssetHelperWindow));
            thisWindow.titleContent = new GUIContent("FBX动画资源提取");
            thisWindow.position = new Rect(Screen.width / 2f, Screen.height / 2f, 600, 800);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Select Search Path");
            EditorGUILayout.TextField(assetSrcFolderPath);
            if (GUILayout.Button("Select"))
            {
                assetSrcFolderPath = EditorUtility.OpenFolderPanel("Select Search Path", assetSrcFolderPath, "");
            }

            EditorGUILayout.EndHorizontal();

            UseSourcePath = EditorGUILayout.Toggle("Use Source Path",UseSourcePath);
            if (!UseSourcePath)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Select Export Path");
                EditorGUILayout.TextField(assetDstFolderPath);
                if (GUILayout.Button("Select"))
                {
                    assetDstFolderPath = EditorUtility.OpenFolderPanel("Select Search Path", assetDstFolderPath, "");
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Start") && assetSrcFolderPath != null && (assetDstFolderPath != null || UseSourcePath))
            {
                Seperate();
            }
        }

        private static void Seperate()
        {
            var appPath = Application.dataPath.Replace("\\", "/");

            if (!UseSourcePath)
            {
                if (assetDstFolderPath.Replace("\\", "/").Contains(appPath))
                {
                    assetDstFolderPath = $"Assets/{assetDstFolderPath.Replace(appPath, "")}";
                }
                if (!Directory.Exists(assetDstFolderPath))
                {
                    Directory.CreateDirectory(assetDstFolderPath);
                }
            }
            var fbxList = Directory.EnumerateFiles(assetSrcFolderPath, "*.fbx", SearchOption.AllDirectories).ToList();
            LogManager.Log(LOGTag,assetSrcFolderPath,UseSourcePath ? "null" : assetDstFolderPath,fbxList.Count);
            var dstPath = UseSourcePath ? string.Empty : assetDstFolderPath;
            foreach (var path in fbxList)
            {
                var relPath = $"Assets/{path.Replace(appPath,"")}";
                relPath = relPath.Replace("\\", "/");
                if (UseSourcePath)
                {
                    dstPath = relPath.Substring(0, relPath.LastIndexOf("/", StringComparison.Ordinal));
                }
                LogManager.Log(LOGTag,path,dstPath);
                
                var srcObjs = AssetDatabase.LoadAllAssetsAtPath(relPath);
                foreach (var obj in srcObjs)
                {
                    if (obj == null || obj is not AnimationClip || obj.name.StartsWith("__preview__"))
                    {
                        continue;
                    }
                    
                    var dts = dstPath + "/" + obj.name + ".anim";
                    var dstclip = AssetDatabase.LoadAssetAtPath(dts, typeof(AnimationClip)) as AnimationClip;
                    if (dstclip != null)
                    {
                        AssetDatabase.DeleteAsset(dts);
                    }
                    
                    var tempClip = new AnimationClip();
                    EditorUtility.CopySerialized(obj, tempClip);
                    AssetDatabase.CreateAsset(tempClip, dts);
                    LogManager.Log(obj.name);
                }
                
            }
        }
    }
}