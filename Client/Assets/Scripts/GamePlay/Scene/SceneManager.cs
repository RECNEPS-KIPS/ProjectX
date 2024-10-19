using System.Collections.Generic;
using Framework.Core.Manager.Config;
using Framework.Core.Manager.ResourcesLoad;
using Framework.Core.Singleton;

namespace GamePlay
{
    public enum SceneDef
    {
        Lobby = 10001,
    }
    public class SceneManager: Singleton<SceneManager>
    {
        private const string LOGTag = "LevelManager";

        private static Dictionary<int, dynamic> _sceneCfMap;

        private static Dictionary<int, dynamic> SceneCfMap
        {
            get
            {
                if (_sceneCfMap != null) return _sceneCfMap;
                _sceneCfMap = new Dictionary<int, dynamic>();
                var cfList = ConfigManager.GetConfig(ConfigNameDef.Scene);
                // LogManager.Log(LOGTag,cfList.Count);
                foreach (var cf in cfList)
                {
                    _sceneCfMap.Add(cf["id"],cf);
                    // LogManager.Log(LOGTag,cf);
                }

                return _sceneCfMap;
            }
        }

        public static dynamic GetSceneConfig(int sceneID)
        {
            SceneCfMap.TryGetValue(sceneID,out var cf);
            return cf;
        }

        public static void LoadSceneByID(SceneDef sceneID)
        {
            LoadSceneByID((int)sceneID);
        }

        //同步从assetbundle中加载场景
        public static void LoadSceneByID(int sceneID)
        {
            var sceneCf = GetSceneConfig(sceneID);
            string path = sceneCf["path"];
            ResourcesLoadManager.LoadAssetBundleFile(ResourcesLoadManager.GetAssetBundleName(path));
            UnityEngine.SceneManagement.SceneManager.LoadScene(path);
        }
    }
}