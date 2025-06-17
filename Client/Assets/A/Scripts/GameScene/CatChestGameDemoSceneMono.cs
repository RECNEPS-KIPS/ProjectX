using System.Collections;
using System.Collections.Generic;
using GameFramework;
using UnityEngine;

namespace GameLogic
{
    public class CatChestGameDemoSceneMono : GameSceneMonoBase
    {
        protected override void Awake()
        {
            base.Awake();

            m_gameModuleManager.AddModule(new GameResourceLoadModule());
            m_gameModuleManager.AddModule(new GameConfigModule());
            m_gameModuleManager.AddModule(new GameSaveModule());
        }

        protected IEnumerator Start()
        {
            LoadSaveData();


            yield return null;
        }

        private void LoadSaveData()
        {
            var catChestGameSaveVO = m_gameModuleManager.GetModule<GameSaveModule>().LoadSaveData<CatChestGameSaveVO>("CatChestGameSaveVO", null);
        }


        private void Update()
        {
            // 游戏逻辑更新
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(CatChestGameDemoSceneMono))]
        public class CatChestGameDemoSceneMonoEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                CatChestGameDemoSceneMono catChestGameDemoSceneMono = (CatChestGameDemoSceneMono)target;

                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.LabelField("开发工具", UnityEditor.EditorStyles.boldLabel);

                if (GUILayout.Button("删除存档数据"))
                {
                    if (UnityEditor.EditorUtility.DisplayDialog("警告", "确定要删除所有存档数据吗？此操作不可撤销！", "确定", "取消"))
                    {
                        GameSaveModule.DeleteAllSaves();
                        UnityEditor.EditorUtility.DisplayDialog("提示", "存档数据已删除", "确定");
                    }
                }
            }
        }
#endif

    }
}