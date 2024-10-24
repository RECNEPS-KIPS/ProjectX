using UnityEditor;
using UnityEngine;

namespace Framework.Common
{
    [CustomEditor(typeof(MeshSave))]
    public class MeshSaveEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MeshSave myScript = (MeshSave)target;

            if (GUILayout.Button("提取Mesh"))
            {
                myScript.SaveAsset();
            }
        }
    }
}