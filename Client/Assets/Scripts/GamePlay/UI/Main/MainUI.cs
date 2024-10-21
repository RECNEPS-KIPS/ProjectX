// author:KIPKIPS
// date:2024.10.19 14:30
// describe:剧情界面

using Framework;
using Framework.Common;
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
            
            VBind("Hungry_Text",$"{hp}{CommonUtils.SetRichFontSize("%",9)}");
            VBind("Stamina_Text",$"{hp}{CommonUtils.SetRichFontSize("%",9)}");
            VBind("Thirsty_Text",$"{hp}{CommonUtils.SetRichFontSize("%",9)}");
            
            VBind("HP_Inner_Color",ColorUtils.GetColorByKey(lowHP ? ColorDef.LOW_HP :ColorDef.HEALTHY_HP));
            VBind("HP_Wrapper_Color",ColorUtils.GetColorByKey(lowHP ? ColorDef.LOW_HP :ColorDef.HEALTHY_HP));
        }

        public override void OnEnter(dynamic args)
        {
            var hp = 20;
            var lowHP = hp / 100f <= .3;
            Bind("HP_Percent",.2f);
            Bind("Hungry_Percent",.2f);
            Bind("Stamina_Percent",.2f);
            Bind("Thirsty_Percent",.2f);
            
            Bind("Hungry_Text",$"{hp}{CommonUtils.SetRichFontSize("%",9)}");
            Bind("Stamina_Text",$"{hp}{CommonUtils.SetRichFontSize("%",9)}");
            Bind("Thirsty_Text",$"{hp}{CommonUtils.SetRichFontSize("%",9)}");
            
            Bind("HP_Inner_Color",ColorUtils.GetColorByKey(lowHP ? ColorDef.LOW_HP :ColorDef.HEALTHY_HP));
            Bind("HP_Wrapper_Color",ColorUtils.GetColorByKey(lowHP ? ColorDef.LOW_HP :ColorDef.HEALTHY_HP));
        }
    }
}