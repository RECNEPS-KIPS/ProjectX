using System;
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
    
    [Serializable]
    public class ChunkColliderInfo
    {
        [SerializeField]public float PositionX;
        [SerializeField]public float PositionY;
        [SerializeField]public float PositionZ;
        [SerializeField]public float SizeX;
        [SerializeField]public float SizeY;
        [SerializeField]public float SizeZ;

        public override string ToString()
        {
            return $"ChunkColliderInfo Position:{new Vector3(PositionX,PositionY,PositionZ)},Size:{new Vector3(SizeX,SizeY,SizeZ)}";
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
                LoadTerrainChunk(worldName, i);
            }

            callback?.Invoke();
        }
        private static void LoadTerrainChunk(string worldName, int index)
        {
            var chunkDir = $"Chunk{DEF.TerrainSplitChar}{index}";
            var saveDir = $"{DEF.RESOURCES_ASSETS_PATH}/Worlds/{worldName}/{chunkDir}";
            var td = ResourcesLoadManager.LoadAsset<TerrainData>($"{saveDir}/Terrain.asset");
            var chunkRoot = new GameObject();
            chunkRoot.transform.SetParent(envRoot);
            chunkRoot.transform.localScale = Vector3.one;
            chunkRoot.transform.localRotation = Quaternion.identity;
            // chunkRoot.transform.localPosition = new Vector3(row * td.size.x, 0, col * td.size.z);
            // chunkRoot.name = $"Chunk{DEF.TerrainSplitChar}{row}{DEF.TerrainSplitChar}{col}";
            
            var go = Terrain.CreateTerrainGameObject(td);
            go.transform.SetParent(chunkRoot.transform);
            go.name = "Terrain";
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            go.gameObject.isStatic = true;
            go.layer = LayerMask.NameToLayer("Ground");

        }
    }
}