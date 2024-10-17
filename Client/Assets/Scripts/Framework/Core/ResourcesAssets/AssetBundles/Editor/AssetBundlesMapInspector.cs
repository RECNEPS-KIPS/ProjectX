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
        private Dictionary<string,List<string>> dict;
        public void OnEnable()
        {
            _assetMap = (AssetBundlesMap)target;
            dict = new Dictionary<string, List<string>>();

            for (int i = 0; i < _assetMap.map.Count; i++)
            {
                var abName = _assetMap.map[i].bundleName;
                if (!dict.ContainsKey(abName))
                {
                    dict.Add(abName,_assetMap.map[i].assetBundlesMap);
                }
                LogManager.Log("AssetBundlesMapInspector",abName);
            }
        }
        
        public override void OnInspectorGUI() {
            // EditorGUILayout.BeginHorizontal();
            //
            // EditorGUILayout.EndHorizontal();
            // GUILayout.Space(5);
            foreach (var kvp in dict)
            {
                EditorGUILayout.BeginVertical();
                GUILayout.Label($"BundleName:{kvp.Key}");
                foreach (var assetName in kvp.Value)
                {
                    // EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"  AssetName:{assetName}");
                    // EditorGUILayout.EndHorizontal();
                }
                GUILayout.Space(10);
                EditorGUILayout.EndVertical();
            }

        }
    }
}