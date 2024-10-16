// author:KIPKIPS
// date:2024.10.16 19:19
// describe:开始界面
using Framework;
using Framework.Core.Manager.Language;
using Framework.Core.Manager.UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class StartWindow : BaseWindow
    {
        public override void OnInit()
        {
            base.OnInit();
            VBind("TitleText",LanguageManager.Instance.GetText(LanguageManager.EStringTable.Start,"GAME_NAME"));
            VBind("StartBtnText",LanguageManager.Instance.GetText(LanguageManager.EStringTable.Start,"START_GAME_TEXT"));
            

            MBind("OnStartBtn", OnStartBtn);
            
        }

        void OnStartBtn()
        {
            UIManager.Instance.Close(WindowNameDef.StartWindow);
            LogManager.Log("OnGameStartBtnClick");
        }
        public override void OnEnter(dynamic args) {
            base.OnEnter();
            LogManager.Log("StartWindow -> OnEnter");
        }
    }
}