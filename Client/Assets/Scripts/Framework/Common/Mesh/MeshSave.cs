using System;
using UnityEditor;
using UnityEngine;

namespace Framework.Common
{
    public class MeshSave : MonoBehaviour
    {
#if UNITY_EDITOR
        public void SaveAsset()
        {
            LogManager.Log("开始提取mesh");
            try
            {
                Mesh mesh = this.GetComponent<MeshFilter>().mesh;
                if (mesh != null) {
                    AssetDatabase.CreateAsset(mesh, "Assets/MeshSave/" + name + ".asset");
                    LogManager.Log("提取mesh成功：提取_" + name);
                }
                else
                    LogManager.LogWarning("提取mesh失败：无MeshFilter组件");
            }
            catch (Exception e)
            {
                LogManager.LogWarning("提取mesh失败：" + e.ToString());
            }
        }
#endif
    }
}