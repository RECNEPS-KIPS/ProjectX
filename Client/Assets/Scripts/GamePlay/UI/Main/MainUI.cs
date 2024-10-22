// author:KIPKIPS
// date:2024.10.19 14:30
// describe:剧情界面

using Framework;
using Framework.Common;
using Framework.Core.Manager;
using Framework.Core.Manager.Config;
using Framework.Core.Manager.Event;
using Framework.Core.Manager.Language;
using Framework.Core.Manager.ResourcesLoad;
using Framework.Core.Manager.UI;
using GamePlay;
using GamePlay.InGame.Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GamePlay.UI
{
    public class MainUI : BaseUI
    {
        public override void OnInit()
        {
            base.OnInit();
            var hp = 20;
            var lowHP = hp / 100f <= .3;
            VBind("HP_Percent",.2f);
            VBind("Hungry_Percent",.2f);
            VBind("Stamina_Percent",.2f);
            VBind("Thirsty_Percent",.2f);
            
            VBind("HP_Text",$"{hp}/{CommonUtils.GetFormatNum(10000000)}");
            VBind("Hungry_Text",$"{hp}{CommonUtils.SetRichFontSize("%",9)}");
            VBind("Stamina_Text",$"{hp}{CommonUtils.SetRichFontSize("%",9)}");
            VBind("Thirsty_Text",$"{hp}{CommonUtils.SetRichFontSize("%",9)}");
            
            VBind("HP_Inner_Color",ColorUtils.GetColorByKey(lowHP ? ColorDef.LOW_HP :ColorDef.HEALTHY_HP));
            VBind("HP_Wrapper_Color",ColorUtils.GetColorByKey(lowHP ? ColorDef.LOW_HP :ColorDef.HEALTHY_HP));
        }

        private void OnPlayerAttrUpdate()
        {
            var attr = PlayerManager.Instance.PlayerAttr;
        }

        public override void OnEnter(dynamic args)
        {
            EventManager.Register(EEvent.PLAYER_ATTR_UPDATE, OnPlayerAttrUpdate);
        }
        
        public override void OnExit()
        {
            EventManager.Remove(EEvent.PLAYER_ATTR_UPDATE,OnPlayerAttrUpdate);
        }
    }
}