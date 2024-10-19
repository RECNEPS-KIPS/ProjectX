// author:KIPKIPS
// date:2024.10.19 14:30
// describe:剧情界面

using Framework;
using Framework.Core.Manager.Config;
using Framework.Core.Manager.Language;
using Framework.Core.Manager.ResourcesLoad;
using Framework.Core.Manager.UI;
using GamePlay;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GamePlay.UI
{
    public class PlotUI : BaseUI
    {
        public override void OnInit()
        {
            base.OnInit();

            MBind("OnSkipBtn", OnSkipBtn);
        }

        void OnSkipBtn()
        {
            LogManager.Log("PlotWindow","OnSkipBtn");
            UIManager.Instance.Close(UIDef.PlotUI);
            SceneManager.Instance.LoadSceneByID(SceneDef.Lobby, () =>
            {
                UIManager.Instance.OpenUI(UIDef.LobbyMainUI);
            });
        }

        public override void OnEnter(dynamic args)
        {
        }
    }
}