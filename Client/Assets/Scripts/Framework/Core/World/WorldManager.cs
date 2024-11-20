using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Framework.Common;
using Framework.Core.Manager.Config;
using Framework.Core.Manager.ResourcesLoad;
using Framework.Core.Singleton;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Core.World
{
    [Serializable]
    public class WorldData
    {
        [SerializeField]public float TerrainHeight;
        [SerializeField]public int PiecesPerAxis;//长宽的数量
        [SerializeField]public float ChunkSizeX;//长宽的尺寸
        [SerializeField]public float ChunkSizeY;//长宽的尺寸

        public override string ToString()
        {
            return $"TerrainHeight:{TerrainHeight},PiecesPerAxis:{PiecesPerAxis},ChunkSizeX:{ChunkSizeX},ChunkSizeY:{ChunkSizeY}";
        }
    }

    //地形数据
    [Serializable]
    public class TerrainInfo
    {
        [SerializeField]public float X;
        [SerializeField]public float Y;
        [SerializeField]public float Z;
    }
    
    //碰撞盒数据
    [Serializable]
    public class ColliderInfo
    {
        [SerializeField]public float PositionX;
        [SerializeField]public float PositionY;
        [SerializeField]public float PositionZ;
        [SerializeField]public float SizeX;
        [SerializeField]public float SizeY;
        [SerializeField]public float SizeZ;

        public override string ToString()
        {
            return $"ColliderInfo Position:{new Vector3(PositionX,PositionY,PositionZ)},Size:{new Vector3(SizeX,SizeY,SizeZ)}";
        }
    }
    
    // [MonoSingletonPath("[Manager]/GameManager")]
    public class WorldManager : Singleton<WorldManager>
    {
        private const string LOGTag = "WorldManager";

        // private static TerrainHandler terrainHandler;
        private static Transform _envRoot;
        private static Transform envRoot
        {
            get
            {
                var root = GameObject.Find(DEF.ENV_ROOT);
                _envRoot = root == null ? CommonUtils.CreateNode(DEF.ENV_ROOT) : root.transform;
                return _envRoot;
            }
        }

        public void Launch()
        {
            // terrainHandler = new TerrainHandler();
        }

        public static void EnterWorld(EWorld worldID, Action callback = null)
        {
            var cf = ConfigManager.GetConfigByID(EConfig.World, (int)worldID);
            if (cf == null)
            {
               LogManager.LogError(LOGTag,$"WorldID:{worldID} has no config!");
               return;
            }

            // var terrainAssetPath = cf["terrainAssetPath"];
            LoadTerrainChunks(cf["worldName"],callback);
        }
        
        public static void LoadTerrainChunks(string worldName, Action callback = null)
        {
            var worldDataPath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/WorldData.bytes";
            var assetData = ResourcesLoadManager.LoadAsset<TextAsset>(worldDataPath);
            var data = BinaryUtils.Bytes2Object<WorldData>(assetData.bytes);

            for (var i = 0; i < data.PiecesPerAxis * data.PiecesPerAxis; i++)
            {
                LoadTerrainChunk(worldName,i);
                LoadItemChunk(worldName, i);
            }

            callback?.Invoke();
        }
        private static void LoadTerrainChunk(string worldName, int index)
        {
            var chunkDir = $"Chunk{DEF.TerrainSplitChar}{index}";
            
            var terrainInfoPath = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}/TerrainInfo.bytes";
            if (!File.Exists(terrainInfoPath))
            {
                LogManager.Log(LOGTag, $"There is no terrain info,path:{terrainInfoPath}");
                return;
            }
            var assetData = ResourcesLoadManager.LoadAsset<TextAsset>(terrainInfoPath);
            var data = BinaryUtils.Bytes2Object<TerrainInfo>(assetData.bytes);
            
            var saveDir = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}";
            var td = ResourcesLoadManager.LoadAsset<TerrainData>($"{saveDir}/Terrain.asset");
            // var chunkName = $"Chunk{DEF.TerrainSplitChar}{index}";
            var chunkRoot = envRoot.Find(chunkDir);
            if (chunkRoot == null)
            {
                chunkRoot = new GameObject(chunkDir).transform;
            }
            chunkRoot.SetParent(envRoot);
            chunkRoot.localScale = Vector3.one;
            chunkRoot.localRotation = Quaternion.identity;
            // var x = Mathf.FloorToInt(index / (float)piecesPerAxis);
            // var y = index - x * piecesPerAxis;
            // chunkRoot.transform.localPosition = new Vector3(x * td.size.x, 0, y * td.size.z);
            chunkRoot.transform.localPosition = new Vector3(data.X,data.Y,data.Z);
            
            var go = Terrain.CreateTerrainGameObject(td);
            go.transform.SetParent(chunkRoot);
            go.name = "Terrain";
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            go.gameObject.isStatic = true;
            go.layer = LayerMask.NameToLayer("Ground");
        }

        private static void LoadItemChunk(string worldName, int index)
        {
            var chunkDir = $"Chunk{DEF.TerrainSplitChar}{index}";
            var saveDir = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}";
            var chunkRoot = envRoot.Find(chunkDir);
            if (chunkRoot == null)
            {
                chunkRoot = new GameObject(chunkDir).transform;
            }

            var item = ResourcesLoadManager.LoadAsset<GameObject>($"{saveDir}/ItemChunk.prefab");
            if (item != null)
            {
                var go = Object.Instantiate(item, chunkRoot, true);

                go.name = "ItemChunk";
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
                go.gameObject.isStatic = true;
                go.layer = LayerMask.NameToLayer("Ground");
            }
        }
    }
}