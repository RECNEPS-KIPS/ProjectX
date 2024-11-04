using System.Collections.Generic;
using Framework.Core.Manager.Config;
using Framework.Core.Manager.Event;
using Framework.Core.Manager.ResourcesLoad;
using Framework.Core.Singleton;
using Framework.Core.SpaceSegment;
using GamePlay.Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.Core.Manager.Scene
{
    [MonoSingletonPath("[Manager]/SceneManager")]
    public class SceneManager: MonoSingleton<SceneManager>
    {
        private const string LOGTag = "SceneManager";
        
        private static Dictionary<string, int> _scenePathIDMap;
        
        private static Dictionary<string, int> ScenePathIDMap => _scenePathIDMap ??= new Dictionary<string, int>();
        private static Dictionary<int, dynamic> _sceneCfMap;

        //scene id-cf
        private static Dictionary<int, dynamic> SceneCfMap
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
        private UnityAction<UnityEngine.SceneManagement.Scene, UnityEngine.SceneManagement.LoadSceneMode> SceneLoadFinished;
        private UnityAction<UnityEngine.SceneManagement.Scene> SceneUnloadFinished;
        private static Dictionary<string, UnityAction> _loadedCallbackMap;

        private static Dictionary<string, UnityAction> LoadedCallbackMap
        {
            get
            {
                _loadedCallbackMap ??= new Dictionary<string, UnityAction>();
                return _loadedCallbackMap;
            }
        }
        public void Launch()
        {
        }

        public override void Initialize()
        {

            LogManager.Log(LOGTag,$"Register scene load finished callback");
            SceneUnloadFinished = scene =>
            {
                if (LoadedCallbackMap.TryGetValue(scene.path,out var callback))
                {
                    callback?.Invoke();
                }
                dynamic cf = null;
                if (ScenePathIDMap.TryGetValue(scene.path, out var sceneID))
                {
                    cf = GetSceneConfig(sceneID);
                }
                EventManager.Dispatch(EEvent.SCENE_UNLOAD_FINISHED,cf);
            };
            SceneLoadFinished = (scene, _) =>
            {
                LogManager.Log(LOGTag,$"Scene load finished === name:{scene.path}");
                // canUpdateSceneDetector = false;
                if (LoadedCallbackMap.TryGetValue(scene.path,out var callback))
                {
                    callback?.Invoke();
                }

                dynamic cf = null;
                if (ScenePathIDMap.TryGetValue(scene.path, out var sceneID))
                {
                    cf = GetSceneConfig(sceneID);
                }
                EventManager.Dispatch(EEvent.SCENE_LOAD_FINISHED,cf);
            };
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += SceneUnloadFinished;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoadFinished;
        }

        public override void Dispose()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneLoadFinished;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= SceneUnloadFinished;
        }

        public static dynamic GetSceneConfig(int sceneID)
        {
            SceneCfMap.TryGetValue(sceneID,out var cf);
            return cf;
        }

        public static void LoadSceneByID(EScene sceneID,UnityAction callback = null)
        {
            var sceneCf = GetSceneConfig((int)sceneID);
            if (sceneCf == null) return;
            LoadSceneByID((int)sceneID,callback);
        }

        //同步从assetbundle中加载场景
        public static void LoadSceneByID(int sceneID,UnityAction callback = null)
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