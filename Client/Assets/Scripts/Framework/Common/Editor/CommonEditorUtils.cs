using System;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.Common
{
    public static class CommonEditorUtils
    {
        [MenuItem("Tools/启动游戏",false,-9999999)]
        private static void GameLaunch()
        {
            const string path = "Assets/ResourcesAssets/Scenes/Launch.unity";
            var sceneName = Path.GetFileNameWithoutExtension(path);
            var bIsCurScene = SceneManager.GetActiveScene().name.Equals(sceneName);//是否为当前场景
            EditorApplication.ExecuteMenuItem("Edit/Play");
            if (!bIsCurScene)
            {
                EditorSceneManager.OpenScene(path);
            }
            EditorApplication.ExecuteMenuItem("Edit/Play");
        }
        
        [MenuItem("Tools/Mesh导出",false)]
        public static void ExportMeshAsset() {
            var obj = Selection.activeObject;
            try
            {
                var mesh = obj.GetComponent<MeshFilter>().mesh;
                if (mesh != null) {
                    var path = $"Assets/{obj.name}_{DateTime.Now.Millisecond}.asset";
                    AssetDatabase.CreateAsset(mesh, path);
                    LogManager.Log("提取mesh成功：提取_" + path);
                }
                else
                {
                    LogManager.LogWarning("提取mesh失败：无MeshFilter组件");
                }
            }
            catch (Exception e)
            {
                LogManager.LogWarning("提取mesh失败：" + e.ToString());
            }
        }
    }
    
}