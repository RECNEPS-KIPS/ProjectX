using System;
using System.Data;
using Framework;
using Framework.Common;
using Framework.Core.Manager.Config;
using Framework.Core.Manager.Event;
using Framework.Core.Manager.ResourcesLoad;
using Framework.Core.Singleton;
using Framework.Core.SpaceSegment;
using GamePlay.Character;
using UnityEngine;
using CharacterController = GamePlay.Character.CharacterController;

namespace GamePlay.Player
{
    public enum ECharacter
    {
        Unknown = 10001,
        BagBoy = 10002,
    }
    
    [MonoSingletonPath("[Manager]/PlayerManager")]
    public class PlayerManager:MonoSingleton<PlayerManager>
    {
        [NonSerialized]
        public const int PROTAGONIST_ID = (int)ECharacter.Unknown;
        
        private const string LOGTag = "PlayerManager";

        [SerializeField]
        private PlayerAttr _playerAttr;

        public PlayerAttr PlayerAttr
        {
            get
            {
                if (_playerAttr != null) return _playerAttr;
                _playerAttr = new PlayerAttr();
                _playerAttr.InitAttrValues();

                return _playerAttr;
            }
        }
        
        [SerializeField]
        private PlayerStatus _playerStatus;

        public PlayerStatus PlayerStatus
        {
            get
            {
                if (_playerStatus != null) return _playerStatus;
                _playerStatus = new PlayerStatus();
                _playerStatus.InitStatusValues();

                return _playerStatus;
            }
        }
        
        public CharacterController CharacterController;

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

        void OnSceneLoadFinished(dynamic sceneCf)
        {
            // if (CharacterController != null)
            // {
            //     return;
            // }
            // LoadPlayerController(sceneCf);
        }

        public Transform CharacterControllerRoot;

        public void LoadPlayerController(Vector3 initPlayerPos = default)
        {
            LogManager.Log(LOGTag,"LoadPlayerController");
            var playerCf = ConfigManager.GetConfigByID(EConfig.Character, PlayerManager.PROTAGONIST_ID);
            var modelPath = playerCf["modelPath"];
            var ctrlType = playerCf["ctrlType"];
            var ctrlCfList = ConfigManager.GetConfig(EConfig.CharacterController);
            dynamic ctrlCf = null;
            for (var i = 0; i < ctrlCfList.Count; i++)
            {
                if (ctrlCfList[i]["ctrlType"] != ctrlType) continue;
                ctrlCf = ctrlCfList[i];
                break;
            }

            if (ctrlCf != null)
            {
                LogManager.Log(LOGTag,"CharacterCtrl initPos:",initPlayerPos);
                GameObject ctrlGo = Instantiate(ResourcesLoadManager.LoadAsset<GameObject>(ctrlCf["path"]), initPlayerPos, Quaternion.identity);
                CharacterController = ctrlGo.GetComponent<CharacterController>();
                ctrlGo.name = "CharacterController";
                CharacterControllerRoot = ctrlGo.transform;
                DontDestroyOnLoad(CharacterControllerRoot);
                if ((DEF.ECharacterControllerType)ctrlCf["ctrlType"] != DEF.ECharacterControllerType.FPS)
                {
                    GameObject modelGo = Instantiate(ResourcesLoadManager.LoadAsset<GameObject>(modelPath));
                    var animCtrl = modelGo.AddComponent<AnimationControl>();
                    animCtrl.Init(ctrlGo.GetComponent<MovementController>());
                    var mt = modelGo.transform;
                    
                    // LogManager.Log(LOGTag,"LoadPlayerController",mt==null,cc==null);
                    CommonUtils.ResetGO(mt,CharacterController.ModelMountRoot);
                }
            }
            EventManager.Dispatch(EEvent.PLAYER_LOAD_FINISHED);
        }

        private void Update()
        {
            UpdateStatus();
            UpdateAttr();
        }

        private void UpdateStatus()
        {
            
        }

        private void UpdateAttr()
        {
            
        }   
    }
}