// author:KIPKIPS
// date:2024.10.19 14:30
// describe:剧情界面

using Framework.Core.Manager.Config;
using Framework.Core.Manager.Scene;
using Framework.Core.Manager.UI;
using Framework.Core.World;
using GamePlay.Player;
using UnityEngine;

// using UnityEngine.SceneManagement;

namespace GamePlay.UI
{
    public class PlotUI : BaseUI
    {
        public override void OnInit()
        {
            base.OnInit();

            MBind("OnSkipBtn", OnSkipBtn);
        }

        private void OnSkipBtn()
        {
            LogManager.Log("PlotWindow","OnSkipBtn");
            UIManager.Close(EUI.PlotUI);
            SceneManager.LoadSceneByID(EScene.MainWorld, () =>
            {
                UIManager.OpenUI(EUI.MainUI);
                WorldManager.EnterWorld(EWorld.MainWorld, () =>
                {
                    var initPos = Vector3.zero;
                    var cf = ConfigManager.GetConfigByID(EConfig.World, (int)EWorld.MainWorld);
                    if (cf != null)
                    {
                        initPos = cf["initPos"];
                    }
                    PlayerManager.Instance.LoadPlayerController(initPos);
                });
            });
        }

        public override void OnEnter(dynamic args)
        {
        }
    }
}