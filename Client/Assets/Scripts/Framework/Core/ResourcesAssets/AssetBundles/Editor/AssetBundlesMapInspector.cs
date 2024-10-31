using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.Core.ResourcesAssets
{
    [CustomEditor(typeof(AssetBundlesMap))]
    public class AssetBundlesMapInspector: Editor
    {
        private AssetBundlesMap _assetMap;
        private AssetBundlesMap assetMap {
            get {
                _assetMap ??= (AssetBundlesMap)target;
                return _assetMap;
            }
        }
        private Dictionary<string,List<string>> dict;
        private Dictionary<string, List<string>> Dict {
            get {
                dict ??= new Dictionary<string, List<string>>();
                for (var i = 0; i < assetMap.map.Count; i++)
                {
                    var abName = assetMap.map[i].bundleName;
                    if (!dict.ContainsKey(abName))
                    {
                        dict.Add(abName,assetMap.map[i].assetBundlesMap);
                    }
                    // LogManager.Log("AssetBundlesMapInspector",abName);
                }
                return dict;
            }
        }

        public override void OnInspectorGUI() {
            // GUILayout.Space(5);
            foreach (var kvp in Dict)
            {
                EditorGUILayout.BeginVertical();
                GUILayout.Label($"BundleName:{kvp.Key}");
                foreach (var assetName in kvp.Value)
                {
                    GUILayout.Label($"  AssetName:{assetName}");
                }
                GUILayout.Space(10);
                EditorGUILayout.EndVertical();
            }

        }
    }
}