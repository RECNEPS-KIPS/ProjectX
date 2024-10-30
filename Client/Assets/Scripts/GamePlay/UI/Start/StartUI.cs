// author:KIPKIPS
// date:2024.10.16 19:19
// describe:开始界面

using Framework.Core.Manager.Language;
using Framework.Core.Manager.UI;

namespace GamePlay.UI
{
    public class StartUI : BaseUI
    {
        public override void OnInit()
        {
            base.OnInit();
            // LogManager.Log("StartWindow",LanguageManager.Instance.GetText(LanguageManager.EStringTable.Start, "GAME_NAME"));
            VBind("TitleText", LanguageManager.Instance.GetText(LanguageManager.EStringTable.Start, "GAME_NAME"));
            VBind("StartBtnText", LanguageManager.Instance.GetText(LanguageManager.EStringTable.Start, "START_GAME_TEXT"));


            MBind("OnStartBtn", OnStartBtn);
        }

        void OnStartBtn()
        {
            UIManager.Close(EUI.StartUI);
            LogManager.Log("OnGameStartBtnClick");
        }

        public override void OnEnter(dynamic args)
        {
            Bind("TitleText", LanguageManager.Instance.GetText(LanguageManager.EStringTable.Start, "GAME_NAME"));
            base.OnEnter();
            LogManager.Log("StartWindow -> OnEnter");
        }
    }
}