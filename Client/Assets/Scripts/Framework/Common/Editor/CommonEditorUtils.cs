using System;
using System.Collections.Generic;
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
        [MenuItem("Tools/场景/启动游戏",false,-9999999)]
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
        [MenuItem("Tools/场景/角色编辑",false,-9999998)]
        private static void CharacterEditorScene()
        {
            const string path = "Assets/Scenes/CharacterEditor.unity";
            var sceneName = Path.GetFileNameWithoutExtension(path);
            var bIsCurScene = SceneManager.GetActiveScene().name.Equals(sceneName);//是否为当前场景
            if (!bIsCurScene)
            {
                EditorSceneManager.OpenScene(path);
            }
        }
        
        [MenuItem("Tools/场景/UI编辑",false,-9999998)]
        private static void UIEditorScene()
        {
            const string path = "Assets/Scenes/UIEditor.unity";
            var sceneName = Path.GetFileNameWithoutExtension(path);
            var bIsCurScene = SceneManager.GetActiveScene().name.Equals(sceneName);//是否为当前场景
            if (!bIsCurScene)
            {
                EditorSceneManager.OpenScene(path);
            }
        }
        
        [MenuItem("Tools/场景/World编辑",false,-9999998)]
        private static void WorldEditorScene()
        {
            const string path = "Assets/Scenes/WorldEditor.unity";
            var sceneName = Path.GetFileNameWithoutExtension(path);
            var bIsCurScene = SceneManager.GetActiveScene().name.Equals(sceneName);//是否为当前场景
            if (!bIsCurScene)
            {
                EditorSceneManager.OpenScene(path);
            }
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
        
        public static bool IsPrefabInstance(UnityEngine.GameObject obj){
            var type = PrefabUtility.GetPrefabAssetType(obj);
            var status = PrefabUtility.GetPrefabInstanceStatus(obj);
            // 是否为预制体实例判断
            if (type == PrefabAssetType.NotAPrefab || status == PrefabInstanceStatus.NotAPrefab)
            {
                return false;
            }
            return true;
        }
        
        public static List<T> FindAssetInFolder<T>(string folder) where T : UnityEngine.Object
        {

            var result = new List<T>();
            //定位到指定文件夹
            if (!Directory.Exists(folder))
            {
                return null;
            }
            var directory = new DirectoryInfo(folder);
 
            //查询该文件夹下的所有文件；
            var files = directory.GetFiles();
            var length = files.Length;
            for (var i = 0; i < length; i++)
            {
                var file = files[i];
 
                //跳过Unity的meta文件（后缀名为.meta）
                if (file.Extension.Contains("meta"))
                    continue;
 
                //根据路径直接拼出对应的文件的相对路径
                var path = $"{folder}/{file.Name}";
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    result.Add(asset);
                }
            }

            return result;
        }
    }
    
}