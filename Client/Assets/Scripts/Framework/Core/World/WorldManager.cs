using Framework.Core.Manager.Config;
using Framework.Core.Singleton;

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

        public static void EnterWorld(EWorld worldID)
        {
            var cf = ConfigManager.GetConfigByID(EConfig.World, (int)worldID);
            if (cf == null)
            {
               LogManager.LogError(LOGTag,$"WorldID:{worldID} has no config!");
               return;
            }

            var terrainAssetPath = cf["terrainAssetPath"];
        }
    }
}