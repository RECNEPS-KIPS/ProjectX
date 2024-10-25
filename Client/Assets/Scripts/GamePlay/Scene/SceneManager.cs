using System.Collections.Generic;
using Framework.Core.Manager.Config;
using Framework.Core.Manager.Event;
using Framework.Core.Manager.ResourcesLoad;
using Framework.Core.Singleton;
using UnityEngine.Events;

namespace GamePlay.Scene
{
    public class SceneManager: Singleton<SceneManager>
    {
        private const string LOGTag = "SceneManager";
        
        private Dictionary<string, int> _scenePathIDMap;

        private Dictionary<string, int> ScenePathIDMap
        {
            get
            {
                if (_scenePathIDMap == null)
                {
                    _scenePathIDMap = new Dictionary<string, int>();
                }

                return _scenePathIDMap;
            }
        }

        private Dictionary<int, dynamic> _sceneCfMap;

        //sceneid-cf
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
                    ScenePathIDMap.TryAdd(cf["path"],cf["id"]);
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

                dynamic cf = null;
                if (ScenePathIDMap.TryGetValue(scene.path, out var sceneID))
                {
                    cf = GetSceneConfig(sceneID);
                }
                //初始化场景中的物件 按照八叉树结构划分好
                EventManager.Dispatch(EEvent.SCENE_LOAD_FINISHED,cf);
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
            var sceneCf = GetSceneConfig((int)sceneID);
            if (sceneCf != null) {
                //先加载依赖的terrain
                LogManager.Log(LOGTag,$"Load terrain name:{sceneCf["terrainAssetPath"]}");
                
                ResourcesLoadManager.LoadAssetBundleFile(ResourcesLoadManager.GetAssetBundleName(sceneCf["terrainAssetPath"]));
                LoadSceneByID((int)sceneID,callback);
            }
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