using Framework.Common;
using Framework.Core.Manager.Config;
using Framework.Core.Singleton;
using UnityEngine;

namespace Framework.Core.World
{
    public class WorldData
    {
        public float TerrainHeight;
        public int TerrainRowCount;
        public int TerrainColumnCount;
        public float TerrainChunkWidth;
        public float TerrainChunkLength;

        public override string ToString()
        {
            return $"TerrainHeight:{TerrainHeight},TerrainRowCount:{TerrainRowCount},TerrainColumnCount:{TerrainColumnCount},TerrainChunkWidth:{TerrainChunkWidth},TerrainChunkLength:{TerrainChunkLength}";
        }
    }
    // [MonoSingletonPath("[Manager]/GameManager")]
    public class WorldManager : Singleton<WorldManager>
    {
        private const string LOGTag = "WorldManager";

        private static TerrainHandler terrainHandler;
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
            terrainHandler = new TerrainHandler();
        }

        public static void EnterWorld(EWorld worldID)
        {
            var cf = ConfigManager.GetConfigByID(EConfig.World, (int)worldID);
            if (cf == null)
            {
               LogManager.LogError(LOGTag,$"WorldID:{worldID} has no config!");
               return;
            }

            var terrainAssetPath = cf["terrainAssetPath"];
            terrainHandler.LoadSplitTerrain(cf["worldName"],envRoot);
        }
    }
}