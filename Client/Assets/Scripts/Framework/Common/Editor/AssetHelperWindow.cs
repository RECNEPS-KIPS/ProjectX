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
        public static string assetSrcFolderPath = "Assets";
        public static string assetDstFolderPath = "Assets/Export AnimationClip";
        private const string LOGTag = "AssetHelper";
        [MenuItem("Tools/Animation/提取FBX AnimationClip")]
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

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Select Export Path");
            EditorGUILayout.TextField(assetDstFolderPath);
            if (GUILayout.Button("Select"))
            {
                assetDstFolderPath = EditorUtility.OpenFolderPanel("Select Search Path", assetDstFolderPath, "");
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Start") && assetSrcFolderPath != null && assetDstFolderPath != null)
            {
                Seperate();
            }
        }

        private static void Seperate()
        {
            var appPath = Application.dataPath.Replace("\\", "/");
            if (assetDstFolderPath.Replace("\\", "/").Contains(appPath))
            {
                assetDstFolderPath = $"Assets/{assetDstFolderPath.Replace(appPath, "")}";
            }
            
            if (!Directory.Exists(assetDstFolderPath))
            {
                Directory.CreateDirectory(assetDstFolderPath);
            }
            var fbxList = Directory.EnumerateFiles(assetSrcFolderPath, "*.fbx", SearchOption.AllDirectories).ToList();
            LogManager.Log(LOGTag,assetSrcFolderPath,assetDstFolderPath,fbxList.Count);
            foreach (var path in fbxList)
            {
                LogManager.Log(LOGTag,path);
                var relPath = $"Assets/{path.Replace(appPath,"")}";
                var srcclip = AssetDatabase.LoadAssetAtPath(relPath, typeof(AnimationClip)) as AnimationClip;
                if (srcclip == null)
                {
                    continue;
                }
                
                var dstclip = AssetDatabase.LoadAssetAtPath(assetDstFolderPath, typeof(AnimationClip)) as AnimationClip;
                if (dstclip != null)
                {
                    AssetDatabase.DeleteAsset(assetDstFolderPath);
                }
                
                var tempclip = new AnimationClip();
                EditorUtility.CopySerialized(srcclip, tempclip);
                AssetDatabase.CreateAsset(tempclip, assetDstFolderPath + "/" + srcclip.name + ".anim");
            }
        }
    }
}