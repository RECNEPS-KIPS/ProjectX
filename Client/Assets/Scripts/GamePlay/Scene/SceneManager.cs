using System.Collections.Generic;
using Framework.Core.Manager.Config;
using Framework.Core.Manager.Event;
using Framework.Core.Manager.ResourcesLoad;
using Framework.Core.Singleton;
using UnityEngine.Events;

namespace GamePlay
{
    public class SceneManager: Singleton<SceneManager>
    {
        private const string LOGTag = "SceneManager";

        private Dictionary<int, dynamic> _sceneCfMap;

        private Dictionary<int, dynamic> SceneCfMap
        {
            get
            {
                if (_sceneCfMap != null) return _sceneCfMap;
                _sceneCfMap = new Dictionary<int, dynamic>();
                var cfList = ConfigManager.GetConfig(EConfig.Scene);
                // LogManager.Log(LOGTag,cfList.Count);
                foreach (var cf in cfList)
                {
                    _sceneCfMap.Add(cf["id"],cf);
                    // LogManager.Log(LOGTag,cf);
                }

                return _sceneCfMap;
            }
        }

        UnityAction<UnityEngine.SceneManagement.Scene, UnityEngine.SceneManagement.LoadSceneMode> SceneLoadFinished;
        private Dictionary<string, UnityAction> _loadedCallbackMap;

        private Dictionary<string, UnityAction> LoadedCallbackMap
        {
            get
            {
                _loadedCallbackMap ??= new Dictionary<string, UnityAction>();
                return _loadedCallbackMap;
            }
        }
        public void Launch(){}
        public override void Initialize()
        {
            LogManager.Log(LOGTag,$"Register scene load finished callback");
            SceneLoadFinished = (scene, mode) =>
            {
                LogManager.Log(LOGTag,$"Scene load finished === name:{scene.path}");
                if (LoadedCallbackMap.TryGetValue(scene.path,out var callback))
                {
                    callback?.Invoke();
                }
                EventManager.Dispatch(EEvent.SCENE_LOAD_FINISHED);
            };
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoadFinished;
        }

        public override void Dispose()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneLoadFinished;
        }

        public dynamic GetSceneConfig(int sceneID)
        {
            SceneCfMap.TryGetValue(sceneID,out var cf);
            return cf;
        }

        public void LoadSceneByID(EScene sceneID,UnityAction callback = null)
        {
            LoadSceneByID((int)sceneID,callback);
        }

        //同步从assetbundle中加载场景
        public void LoadSceneByID(int sceneID,UnityAction callback = null)
        {
            LogManager.Log(LOGTag,$"Scene load start === name:{sceneID}");
            var sceneCf = GetSceneConfig(sceneID);
            string path = sceneCf["path"];
            if (callback != null)
            {
                if (LoadedCallbackMap.ContainsKey(path))
                {
                    LoadedCallbackMap.Remove(path);
                }
                LoadedCallbackMap.Add(path,callback);
            }
            ResourcesLoadManager.LoadAssetBundleFile(ResourcesLoadManager.GetAssetBundleName(path));
            UnityEngine.SceneManagement.SceneManager.LoadScene(path);
        }
    }
}