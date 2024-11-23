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
        public const int SYSTEM_STANDARD_DPI = 96; //系统默认dpi
        public const int TRUE = 1;
        public const int FALSE = 0;
        
        //UIBinding枚举值的间隔
        public const int BIND_ENUM_GAP = 10000;

        public const string ASSET_BUNDLE_PATH = "Assets/ResourcesAssets/Misc/asset_bundles_map.asset";
        
        public const string ASSET_BUNDLE_RULE_PATH = "Assets/Scripts/Framework/Core/ResourcesAssets/AssetBundles/Editor/asset_bundles_rule.asset";

        public const string RESOURCES_ASSETS_PATH = "Assets/ResourcesAssets";
        
        public const string ASSET_BUNDLE_SUFFIX = "ab";
        
        public const string ENV_ROOT = "[Environment]"; //根节点
        
        // public const string COLLIDER_ROOT = "[Collider]"; //碰撞盒节点
        // public const string ITEM_ROOT = "[Item]"; //场景元素节点
        // public const string TERRAIN_ROOT = "[Terrain]"; //地形节点
        
        public static string LIGHTMAP_TEXTURE_DIR(int idx) => $"Lightmap-{idx}_comp_dir";
        public static string LIGHTMAP_TEXTURE_LIGHT(int idx) => $"Lightmap-{idx}_comp_light";
        public static string LIGHTMAP_TEXTURE_SHADOWMASK(int idx) => $"Lightmap-{idx}_comp_shadowmask";
        
        public const string TerrainSplitChar = "_";
    }
}