using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Application = UnityEngine.Application;

namespace Framework.Common
{
    public static class CommonEditorUtils
    {
        [MenuItem("Tools/GameLaunch",false,-9999999)]
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
    }
    
}