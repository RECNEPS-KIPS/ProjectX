using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

public class GameResourceLoader : MonoBehaviour
{
    private static GameResourceLoader m_instance;
    public static GameResourceLoader Instance
    {
        get
        {
            if (m_instance == null)
            {
                GameObject go = new GameObject("GameResourceLoader");
                m_instance = go.AddComponent<GameResourceLoader>();
                m_instance.GameResourceLoaderInit();
            }
            return m_instance;
        }
    }
    public bool isLoadFromEditor = true;
    
    private void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this;
            GameResourceLoaderInit();
        }
    }

    private void GameResourceLoaderInit()
    {
        DontDestroyOnLoad(gameObject);
    }

    public T LoadResource<T>(string prefabPath,string suffix = "asset") where T : Object
    {
        if (isLoadFromEditor)
        {
            if (typeof(T) == typeof(GameObject))
            {
                suffix = "prefab";
            }else if (typeof(T) == typeof(SpriteAtlas))
            {
                suffix = "spriteatlasv2";
            }
            return AssetDatabase.LoadAssetAtPath<T>($"Assets/GameResources/{prefabPath}.{suffix}");
        }
        else
        {
            Debug.LogError("你还没这个情况下的加载");
            return null;
        }
    }
    
    
}
