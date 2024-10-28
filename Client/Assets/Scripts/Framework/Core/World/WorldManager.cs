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
    public class WorldManager
    {
        
    }
}