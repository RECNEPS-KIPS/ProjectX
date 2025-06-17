using System;
using System.IO;
using System.Collections.Generic;
using GameFramework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace GameLogic
{
    public class GameResourceLoadModule : GameLogicModuleBase
    {
        private SpriteLoader m_spriteLoader;
        
        public override void DisposeModule()
        {
            if (m_spriteLoader != null)
            {
                m_spriteLoader.Clear();
                m_spriteLoader = null;
            }
        }

        public override void InitModule()
        {
            m_spriteLoader = new SpriteLoader(this);
        }

        public T LoadResource<T>(string path, Action<T> callback = null) where T : Object
        {
            if(string.IsNullOrEmpty(path))
            {
                Debug.LogError($"资源路径为空: {path}");
                return null;
            }
            // TODO  接资源加载 Addressbale Bundle
            T asset  = Resources.Load<T>($"{path}");
            Assert.IsNotNull(asset, $"资源加载失败: {path}");
            return asset;
        }
        
        /// <summary>
        /// 统一的Sprite加载接口，先尝试从图集加载，如果找不到再从单独文件加载
        /// </summary>
        /// <param name="spriteName">Sprite名称</param>
        /// <param name="atlasPath">图集路径，为空则不从图集加载</param>
        /// <returns>加载的Sprite</returns>
        public Sprite LoadSprite(string spriteName, string atlasPath = null)
        {
            return m_spriteLoader.LoadSprite(spriteName, atlasPath);
        }
        
        /// <summary>
        /// 预加载图集
        /// </summary>
        /// <param name="atlasPath">图集路径</param>
        public void PreloadAtlas(string atlasPath)
        {
            m_spriteLoader.PreloadAtlas(atlasPath);
        }
        
        /// <summary>
        /// 释放图集
        /// </summary>
        /// <param name="atlasPath">图集路径</param>
        public void ReleaseAtlas(string atlasPath)
        {
            m_spriteLoader.ReleaseAtlas(atlasPath);
        }
    }
    
    /// <summary>
    /// Sprite加载器，负责缓存图集和加载Sprite
    /// </summary>
    public class SpriteLoader
    {
        private GameResourceLoadModule m_resourceLoadModule;
        private Dictionary<string, Sprite> m_spriteCache = new Dictionary<string, Sprite>();
        private Dictionary<string, SpriteAtlas> m_atlasCache = new Dictionary<string, SpriteAtlas>();
        
        public SpriteLoader(GameResourceLoadModule resourceLoadModule)
        {
            m_resourceLoadModule = resourceLoadModule;
        }
        
        /// <summary>
        /// 预加载图集
        /// </summary>
        /// <param name="atlasPath">图集路径</param>
        public void PreloadAtlas(string atlasPath)
        {
            if (string.IsNullOrEmpty(atlasPath))
            {
                Debug.LogError("图集路径为空");
                return;
            }
            
            if (m_atlasCache.ContainsKey(atlasPath))
            {
                return;
            }
            
            // 加载SpriteAtlas图集
            SpriteAtlas atlas = m_resourceLoadModule.LoadResource<SpriteAtlas>(atlasPath);
            if (atlas == null)
            {
                Debug.LogError($"图集加载失败: {atlasPath}");
                return;
            }
            
            m_atlasCache[atlasPath] = atlas;
            Debug.Log($"预加载图集成功: {atlasPath}");
        }
        
        /// <summary>
        /// 统一的Sprite加载接口，先尝试从图集加载，如果找不到再从单独文件加载
        /// </summary>
        /// <param name="spriteName">Sprite名称或路径</param>
        /// <param name="atlasPath">图集路径，为空则不从图集加载</param>
        /// <returns>加载的Sprite</returns>
        public Sprite LoadSprite(string spriteName, string atlasPath = null)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                Debug.LogError("Sprite名称为空");
                return null;
            }
            
            Sprite sprite = null;
            
            // 如果提供了图集路径，先尝试从图集加载
            if (!string.IsNullOrEmpty(atlasPath))
            {
                sprite = LoadSpriteFromAtlas(atlasPath, spriteName);
                
                // 如果从图集中加载成功，直接返回
                if (sprite != null)
                {
                    return sprite;
                }
                
                // 从图集加载失败时，会尝试从单独文件加载
                Debug.Log($"从图集 {atlasPath} 中未找到Sprite: {spriteName}，尝试从单独文件加载");
            }
            
            // 尝试从单独文件加载
            return LoadSpriteFromFile(spriteName);
        }
        
        /// <summary>
        /// 从图集中加载Sprite
        /// </summary>
        /// <param name="atlasPath">图集路径</param>
        /// <param name="spriteName">Sprite名称</param>
        /// <returns>加载的Sprite</returns>
        private Sprite LoadSpriteFromAtlas(string atlasPath, string spriteName)
        {
            if (string.IsNullOrEmpty(atlasPath) || string.IsNullOrEmpty(spriteName))
            {
                Debug.LogError($"图集路径或Sprite名称为空: {atlasPath}, {spriteName}");
                return null;
            }
            
            // 如果图集未加载，先加载图集
            if (!m_atlasCache.ContainsKey(atlasPath))
            {
                PreloadAtlas(atlasPath);
                
                // 加载后仍然不存在，说明加载失败
                if (!m_atlasCache.ContainsKey(atlasPath))
                {
                    return null;
                }
            }
            
            // 从图集中获取Sprite
            SpriteAtlas atlas = m_atlasCache[atlasPath];
            Sprite sprite = atlas.GetSprite(spriteName);
            
            if (sprite == null)
            {
                Debug.LogWarning($"图集中不存在该Sprite: {atlasPath}, {spriteName}");
                return null;
            }
            
            return sprite;
        }
        
        /// <summary>
        /// 从单独文件加载Sprite
        /// </summary>
        /// <param name="spritePath">Sprite路径</param>
        /// <returns>加载的Sprite</returns>
        private Sprite LoadSpriteFromFile(string spritePath)
        {
            if (string.IsNullOrEmpty(spritePath))
            {
                Debug.LogError("Sprite路径为空");
                return null;
            }
            
            // 先查找缓存
            if (m_spriteCache.TryGetValue(spritePath, out Sprite cachedSprite))
            {
                return cachedSprite;
            }
            
            // 加载新的Sprite
            Sprite sprite = m_resourceLoadModule.LoadResource<Sprite>(spritePath);
            if (sprite == null)
            {
                Debug.LogError($"Sprite加载失败: {spritePath}");
                return null;
            }
            
            // 缓存并返回
            m_spriteCache[spritePath] = sprite;
            return sprite;
        }
        
        /// <summary>
        /// 释放图集
        /// </summary>
        /// <param name="atlasPath">图集路径</param>
        public void ReleaseAtlas(string atlasPath)
        {
            if (string.IsNullOrEmpty(atlasPath))
            {
                return;
            }
            
            if (m_atlasCache.ContainsKey(atlasPath))
            {
                m_atlasCache.Remove(atlasPath);
                Debug.Log($"释放图集成功: {atlasPath}");
                
                // 适当情况下可以调用Resources.UnloadUnusedAssets()
                // Resources.UnloadUnusedAssets();
            }
        }
        
        /// <summary>
        /// 清理所有缓存
        /// </summary>
        public void Clear()
        {
            m_spriteCache.Clear();
            m_atlasCache.Clear();
            
            // 释放未使用的资源
            //Resources.UnloadUnusedAssets();
            Debug.Log("已清理所有Sprite缓存");
        }
    }
}   
