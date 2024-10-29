// author:KIPKIPS
// date:2024.10.29 22:33
// describe:
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core.ResourcesAssets
{
    [Serializable]
    public class AssetBundlesRule
    {
        public int Priority;
        public string Path;
        // p
    }
    
    [Serializable]
    [CreateAssetMenu(fileName = "AssetBundlesBuildRule", menuName = "Tools/ResourcesAssets/AssetBundlesBuildRule")]
    public class AssetBundlesBuildRule : ScriptableObject
    {
        [SerializeField]
        public List<AssetBundlesRule> AssetBundlesRules = new();
    }
}