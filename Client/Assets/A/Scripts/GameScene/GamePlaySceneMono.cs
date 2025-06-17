using System.Collections;
using System.Collections.Generic;
using GameFramework;
using UnityEngine;

namespace GameLogic
{
    public class GamePlaySceneMono : GameSceneMonoBase
    {
        [SerializeField]
        private Transform m_animalCustomerRoot;
        [SerializeField]
        private Transform m_foodCellRoot;
        [SerializeField]
        private RequestBubbleMono m_requestBubbleMono;

        // GameData
        private PlayerSaveVO m_playerSaveVO;

        protected override void Awake()
        {
            base.Awake();

            m_gameModuleManager.AddModule(new GameStateModule());
            m_gameModuleManager.AddModule(new GameResourceLoadModule());
            m_gameModuleManager.AddModule(new GameEventModule());
            m_gameModuleManager.AddModule(new GameSaveModule());
            m_gameModuleManager.AddModule(new GameConfigModule());
        }

        private void InitEvent()
        {
            var gameEventModule = GameRoot.Instance.GetGameModule<GameEventModule>();
            gameEventModule.RegisterEvent(GameEventType.ShowRequestBubble, OnShowRequestBubble);
        }

        private void OnShowRequestBubble(GameEventArgBase arg)
        {
            
        }

        private IEnumerator Start()
        {
            // Load GameModule
            // InitEvent
            InitEvent();
            // Load Config
            LoadConfig();
            // Load SaveData
            LoadSaveData();
            yield return null;
        }

        private void Update()
        {
            
        }
        
#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(GamePlaySceneMono))]
        public class GamePlaySceneMonoEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                
                GamePlaySceneMono gamePlaySceneMono = (GamePlaySceneMono)target;
                
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

        private void LoadConfig()
        {
            var gameConfigModule = GameRoot.Instance.GetGameModule<GameConfigModule>();
            gameConfigModule?.LoadConfig();
        }

        private void LoadSaveData()
        {
            m_gameModuleManager.GetModule<GameSaveModule>().LoadSaveData<PlayerSaveVO>("PlayerSaveVO", CreateDefaultSaveData);
        }

        private PlayerSaveVO CreateDefaultSaveData()
        {
            var defaultSaveVo = new PlayerSaveVO();
            defaultSaveVo.coinAmount = 0;
            defaultSaveVo.foodCellSaves = new List<FoodCellSaveVO>();
            var gameConfigModule = GameRoot.Instance.GetGameModule<GameConfigModule>();
            var foodCellConfigVOs = gameConfigModule?.GetConfig<FoodCellConfigVO>();
            foreach (var foodCellConfigVO in foodCellConfigVOs)
            {
                var foodCellSaveVO = new FoodCellSaveVO();
                foodCellSaveVO.foodConfigID = foodCellConfigVO.FoodConfigID;
                foodCellSaveVO.foodCDSpeedLevel = 0;
                foodCellSaveVO.foodMaxAmountLevel = 0;
                foodCellSaveVO.curFoodAmount = 0;
                foodCellSaveVO.isUnlock = foodCellConfigVO.UnlockCoin == 0;
                defaultSaveVo.foodCellSaves.Add(foodCellSaveVO);
            }
            defaultSaveVo.animalCustomerInfos = new List<AnimalCustomerInfo>();
            return defaultSaveVo;
        }

        private void LoadCustomers()
        {

        }
    }

}
