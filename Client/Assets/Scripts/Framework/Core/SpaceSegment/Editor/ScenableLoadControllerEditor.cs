// author:KIPKIPS
// date:2024.10.26 18:36
// describe:
using UnityEngine;
using UnityEditor;

namespace Framework.Core.SpaceSegment
{
    [CustomEditor(typeof(ScenableLoadController))]
    public class ScenableLoadControllerEditor : Editor
    {
        private ScenableLoadController _target;
        private void OnEnable()
        {
            _target = (ScenableLoadController)target;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
#if UNITY_EDITOR
            GUILayout.Label("调试：");
            bool drawTree = GUILayout.Toggle(_target.debugDrawMinDepth >= 0 && _target.debugDrawMaxDepth >= 0, "显示四叉树包围盒");
            if (drawTree == false)
            {
                _target.debugDrawMaxDepth = -1;
                _target.debugDrawMinDepth = -1;
            } else
            {
                _target.debugDrawMaxDepth = _target.debugDrawMaxDepth < 0 ? 0 : _target.debugDrawMaxDepth;
                _target.debugDrawMinDepth = _target.debugDrawMinDepth < 0 ? 0 : _target.debugDrawMinDepth;
            }
            _target.debugDrawObj = GUILayout.Toggle(_target.debugDrawObj, "显示场景对象包围盒");
            if (!drawTree) return;
            GUILayout.Label("显示四叉树深度范围：");
            _target.debugDrawMinDepth = Mathf.Max(0, EditorGUILayout.IntField("最小深度", _target.debugDrawMinDepth));
            _target.debugDrawMaxDepth = Mathf.Max(0, EditorGUILayout.IntField("最大深度", _target.debugDrawMaxDepth));
#endif
        }
    }
}