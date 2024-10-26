// author:KIPKIPS
// date:2024.10.19 14:30
// describe:剧情界面

using Framework.Common;
using Framework.Core.Manager.Event;
using Framework.Core.Manager.UI;
using GamePlay.Player;

namespace GamePlay.UI
{
    public class MainUI : BaseUI
    {
        public override void OnInit()
        {
            base.OnInit();

            VBind("HP_Percent");
            VBind("Hungry_Percent");
            VBind("Stamina_Percent");
            VBind("Thirsty_Percent");
            
            VBind("HP_Text");
            VBind("Hungry_Text");
            VBind("Stamina_Text");
            VBind("Thirsty_Text");
            
            VBind("HP_Inner_Color");
            VBind("HP_Wrapper_Color");
        }

        private void UpdatePlayerAttr()
        {
            var attr = PlayerManager.Instance.PlayerAttr;
            Bind("HP_Percent", (float)attr.CurHP / attr.CurMaxHP);
            Bind("Hungry_Percent", (float)attr.CurHungry / attr.CurMaxHungry);
            Bind("Stamina_Percent", (float)attr.CurStamina / attr.CurMaxStamina);
            Bind("Thirsty_Percent", (float)attr.CurThirsty / attr.CurMaxThirsty);
            
            Bind("HP_Text",$"{CommonUtils.GetFormatNum(attr.CurHP)}/{CommonUtils.GetFormatNum(attr.CurMaxHP)}");
            Bind("Hungry_Text",$"{attr.CurHungry * 100 / attr.CurMaxHungry}{CommonUtils.SetRichFontSize("%",9)}");
            Bind("Stamina_Text",$"{attr.CurStamina * 100 / attr.CurMaxStamina}{CommonUtils.SetRichFontSize("%",9)}");
            Bind("Thirsty_Text",$"{attr.CurThirsty * 100 / attr.CurMaxThirsty}{CommonUtils.SetRichFontSize("%",9)}");
            
            var lowHP = attr.CurHP / 100f <= .3;//todo:走配置
            Bind("HP_Inner_Color",ColorUtils.GetColorByKey(lowHP ? ColorDef.LOW_HP :ColorDef.HEALTHY_HP));
            Bind("HP_Wrapper_Color",ColorUtils.GetColorByKey(lowHP ? ColorDef.LOW_HP :ColorDef.HEALTHY_HP));
        }

        public override void OnEnter(dynamic args)
        {
            EventManager.Register(EEvent.PLAYER_ATTR_UPDATE, UpdatePlayerAttr);
            UpdatePlayerAttr();
        }
        
        public override void OnExit()
        {
            EventManager.Remove(EEvent.PLAYER_ATTR_UPDATE,UpdatePlayerAttr);
        }
    }
}