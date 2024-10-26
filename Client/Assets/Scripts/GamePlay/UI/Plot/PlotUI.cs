// author:KIPKIPS
// date:2024.10.19 14:30
// describe:剧情界面

using Framework.Core.Manager.UI;
using GamePlay.Scene;
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
            UIManager.Instance.Close(EUI.PlotUI);
            SceneManager.Instance.LoadSceneByID(EScene.DeepDesert, () =>
            {
                UIManager.Instance.OpenUI(EUI.MainUI);
            });
        }

        public override void OnEnter(dynamic args)
        {
        }
    }
}