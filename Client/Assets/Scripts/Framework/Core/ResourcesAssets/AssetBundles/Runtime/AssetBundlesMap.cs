// author:KIPKIPS
// date:2022.06.17 00:39
// describe:打包路径生成
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core.ResourcesAssets
{
    [Serializable]
    public struct AssetBundle {
        public List<string> assetBundlesMap;
        public string bundleName;

        public AssetBundle(string name,List<string> map)
        {
            bundleName = name;
            assetBundlesMap = new List<string>();
            foreach (var t in map) {
                assetBundlesMap.Add(t);
            }
        }
    }
    [Serializable]
    [CreateAssetMenu(fileName = "AssetBundlesMap", menuName = "Tools/ResourcesAssets/AssetBundlesMap")]
    public class AssetBundlesMap : ScriptableObject {
        public List<AssetBundle> map;
        public List<AssetBundle> Map => map ??= new List<AssetBundle>();
    }
}