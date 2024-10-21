using UnityEngine;
using UnityEditor;

namespace Michsky.UI.MTP
{
    [CustomEditor(typeof(RootManager))]
    [System.Serializable]
    public class RootManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var rootFolder = serializedObject.FindProperty("rootFolder");

            EditorGUILayout.LabelField(new GUIContent("Root Folder:"), GUILayout.Width(76));
            EditorGUILayout.PropertyField(rootFolder, new GUIContent(""));

            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
        
            if (GUILayout.Button("Update"))
                EditorPrefs.SetString("MTP.StyleCreator.RootFolder", rootFolder.stringValue);

            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            EditorGUILayout.HelpBox("Make sure that the Motion Titles Pack directory matches. " +
                                    "Don't forget to hit update after changing the root. " +
                                    "Example: Parent Folders/Motion Titles Pack/Style Creator/.", MessageType.Info);

            EditorGUILayout.HelpBox("Default directory:\n" +
                                    "Motion Titles Pack/Style Creator/", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }
    }
}