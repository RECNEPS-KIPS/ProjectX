// author:KIPKIPS
// date:2022.05.10 22:54
// describe:定义类

namespace Framework
{
    /// <summary>
    /// 定义类
    /// </summary>
    public static class DEF
    {
        //UIbinding枚举值的间隔
        public static readonly int BIND_ENUM_GAP = 10000;

        public static readonly string ASSET_BUNDLE_PATH = "Assets/ResourcesAssets/Misc/asset_bundles_map.asset";

        public static readonly string RESOURCES_ASSETS_PATH = "Assets/ResourcesAssets";
        
        public static readonly string ASSET_BUNDLE_SUFFIX = "ab";

        public enum ECharacterControllerType
        {
            FPS = 0,
            TPS_A = 1,
            TPS_B = 2,
            TPS_C = 3,
            TOPDOWN = 4,
        }
    }
}