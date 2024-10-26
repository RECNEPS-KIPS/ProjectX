using System.Collections.Generic;
using Framework.Core.Manager.Config;
using Framework.Core.Manager.Event;
using Framework.Core.Manager.ResourcesLoad;
using Framework.Core.Singleton;
using UnityEngine;
using UnityEngine.Events;

namespace GamePlay.Scene
{
    [MonoSingletonPath("[Manager]/SceneManager")]
    public class SceneManager: MonoSingleton<SceneManager>
    {
        private const string LOGTag = "SceneManager";
        
        private Dictionary<string, int> _scenePathIDMap;
        
        [SerializeField]
        private bool DrawGizmos;
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

        //scene id-cf
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
        private UnityAction<UnityEngine.SceneManagement.Scene, UnityEngine.SceneManagement.LoadSceneMode> SceneLoadFinished;
        private Dictionary<string, UnityAction> _loadedCallbackMap;

        private Dictionary<string, UnityAction> LoadedCallbackMap
        {
            get
            {
                _loadedCallbackMap ??= new Dictionary<string, UnityAction>();
                return _loadedCallbackMap;
            }
        }
        // 存储世界中的游戏对象数组
        
        public IOctrable[] worldObjects;
        public int nodeMinSize = 5; // 八叉树的最小节点大小
        
        [SerializeField]
        private Octree octree; // 八叉树对象
        

        // 在每一帧更新时调用
        private void OnDrawGizmos()
        {
            if (Application.isPlaying && octree != null)
            {
                if (DrawGizmos)
                {
                    octree.rootNode.Draw(); // 在运行时绘制八叉树的根节点的包围盒
                }
            }
        }
        public void Launch()
        {
#if UNITY_EDITOR
            DrawGizmos = true;
#endif
        }
        public override void Initialize()
        {
            LogManager.Log(LOGTag,$"Register scene load finished callback");
            SceneLoadFinished = (scene, _) =>
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
                worldObjects = FindObjectsOfType<SceneItem>() as IOctrable[];
                LogManager.Log(LOGTag,$"Scene load IOctrable:{worldObjects.Length} SceneItem:{FindObjectsOfType<SceneItem>().Length}");
                //新场景加载好之后初始化八叉树
                octree = new Octree(worldObjects, nodeMinSize); //创建八叉树对象并初始化
        
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
            if (sceneCf == null) return;
            //先加载依赖的terrain
            LogManager.Log(LOGTag,$"Load terrain name:{sceneCf["terrainAssetPath"]}");
                
            ResourcesLoadManager.LoadAssetBundleFile(ResourcesLoadManager.GetAssetBundleName(sceneCf["terrainAssetPath"]));
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

        public List<IOctrable> CheckBounds(Bounds bounds)
        {
            List<IOctrable> os = new();
            List<OctreeNode> nodes = octree.CheckBounds(bounds);
            foreach (var node in nodes)
            {
                if (!node.isLeaf)
                {
                    continue;
                }
                os.AddRange(node.Octrables);
            }
            return os;
        }
    }
}