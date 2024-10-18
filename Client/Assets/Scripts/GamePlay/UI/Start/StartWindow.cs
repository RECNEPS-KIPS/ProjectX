// author:KIPKIPS
// date:2024.10.16 19:19
// describe:开始界面

using Framework;
using Framework.Core.Manager.Config;
using Framework.Core.Manager.Language;
using Framework.Core.Manager.ResourcesLoad;
using Framework.Core.Manager.UI;
using Framework.GamePlay;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GamePlay.UI
{
    public class StartWindow : BaseWindow
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
            UIManager.Instance.Close(WindowNameDef.StartWindow);
            LogManager.Log("OnGameStartBtnClick");
            
            LevelManager.LoadSceneByID(10001);
        }

        public override void OnEnter(dynamic args)
        {
            Bind("TitleText", LanguageManager.Instance.GetText(LanguageManager.EStringTable.Start, "GAME_NAME"));
            base.OnEnter();
            LogManager.Log("StartWindow -> OnEnter");
        }
    }
}