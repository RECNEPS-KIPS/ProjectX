using System;
using Framework;
using Framework.Common;
using Framework.Core.Manager.Config;
using Framework.Core.Manager.Event;
using Framework.Core.Manager.ResourcesLoad;
using Framework.Core.Singleton;
using GamePlay.InGame.Character;
using UnityEngine;
using CharacterController = GamePlay.InGame.Character.CharacterController;

namespace GamePlay.InGame.Player
{
    [MonoSingletonPath("[Manager]/PlayerManager")]
    public class PlayerManager:MonoSingleton<PlayerManager>
    {
        [NonSerialized]
        public const int PROTAGONIST_ID = 10001;
        
        private const string LOGTag = "PlayerManager";

        [SerializeField]
        private PlayerAttr _playerAttr;

        public PlayerAttr PlayerAttr => _playerAttr ??= new PlayerAttr();

        public void Launch()
        {
            
        }
        public override void Initialize()
        {
            EventManager.Register(EEvent.SCENE_LOAD_FINISHED,OnSceneLoadFinished);
        }
        
        public override void Dispose()
        {
            EventManager.Remove(EEvent.SCENE_LOAD_FINISHED,OnSceneLoadFinished);
        }

        void OnSceneLoadFinished()
        {
            LoadPlayerController();
        }

        private Transform CharacterControllerRoot;

        void LoadPlayerController()
        {
            LogManager.Log(LOGTag,"LoadPlayerController");
            var playerCf = ConfigManager.GetConfigByID(EConfig.Character, PlayerManager.PROTAGONIST_ID);
            var modelPath = playerCf["modelPath"];
            var ctrlType = playerCf["ctrlType"];
            var ctrlCfList = ConfigManager.GetConfig(EConfig.CharacterController);
            dynamic ctrlCf = null;
            for (int i = 0; i < ctrlCfList.Count; i++)
            {
                if (ctrlCfList[i]["ctrlType"] == ctrlType)
                {
                    ctrlCf = ctrlCfList[i];
                    break;
                }
            }

            if (ctrlCf != null)
            {
                GameObject ctrlGo = Instantiate(ResourcesLoadManager.LoadAsset<GameObject>(ctrlCf["path"]), Vector3.zero, Quaternion.identity);
                CharacterController cc = ctrlGo.GetComponent<CharacterController>();
                ctrlGo.name = "CharacterController";
                CharacterControllerRoot = ctrlGo.transform;
                DontDestroyOnLoad(CharacterControllerRoot);
                if ((DEF.ECharacterControllerType)ctrlCf["ctrlType"] != DEF.ECharacterControllerType.FPS)
                {
                    GameObject modelGo = Instantiate(ResourcesLoadManager.LoadAsset<GameObject>(modelPath));
                    var mt = modelGo.transform;
                    
                    LogManager.Log(LOGTag,"LoadPlayerController",mt==null,cc==null);
                    CommonUtils.ResetGO(mt,cc.ModelMountTrs);
                }
            }
        }
    }
}