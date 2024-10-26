using System.Collections.Generic;
using Framework.Core.Singleton;
using Framework.Core.Manager.Config;
using Framework.Core.Manager.ResourcesLoad;
using GamePlay.Scene;

namespace GamePlay
{
    /// <summary>
    /// 多语言管理器
    /// </summary>
    // [MonoSingletonPath("[Manager]/LanguageManager")]
    public class LevelManager : Singleton<LevelManager>
    {
        private const string LOGTag = "LevelManager";

        private Dictionary<int, dynamic> _levelCfMap;

        private Dictionary<int, dynamic> LevelCfMap
        {
            get
            {
                if (_levelCfMap != null) return _levelCfMap;
                _levelCfMap = new Dictionary<int, dynamic>();
                var cfList = ConfigManager.GetConfig(EConfig.LevelSetting);
                // LogManager.Log(LOGTag,cfList.Count);
                foreach (var cf in cfList)
                {
                    _levelCfMap.Add(cf["id"],cf);
                    // LogManager.Log(LOGTag,cf);
                }

                return _levelCfMap;
            }
        }

        public dynamic GetLevelCfByID(int LevelID)
        {
            LevelCfMap.TryGetValue(LevelID,out var cf);
            return cf;
        }

        //同步从assetbundle中加载场景
        public void LoadSceneByID(int levelID)
        {
            var levelCf = GetLevelCfByID(levelID);
            int sceneID = levelCf["sceneID"];
            var sceneCf = SceneManager.Instance.GetSceneConfig(sceneID);
            LogManager.Log(LOGTag,levelCf,sceneCf);
            string path = sceneCf["path"];
            ResourcesLoadManager.LoadAssetBundleFile(ResourcesLoadManager.GetAssetBundleName(path));
            UnityEngine.SceneManagement.SceneManager.LoadScene(path);
        }
    }
}