using System;

using Framework.Core.Manager.Config;
using Framework.Core.Manager.Event;

namespace GamePlay.InGame.Player
{
    [Serializable]
    public class PlayerAttr
    {
        public int BaseDefense;
        public int BaseAttack;
        
        public int BaseHP;
        public int BaseStamina;
        public int BaseThirsty;
        public int BaseHungry;
        
        public int MaxHP;
        public int MaxStamina;
        public int MaxThirsty;
        public int MaxHungry;
        
        public int HP;
        public int Stamina;
        public int Thirsty;
        public int Hungry;
        
        public int GrowthTemp;
        public int Level = 1;
        public int ControllerType;
        public PlayerAttr()
        {
            InitAttrValues();
        }

        /// <summary>
        /// 初始化玩家属性
        /// </summary>
        private void InitAttrValues()
        {
            var characterCf = ConfigManager.GetConfigByID(EConfig.Character, PlayerManager.PROTAGONIST_ID);
            if (characterCf != null)
            {
                BaseHP = characterCf["baseHP"];
                
                EventManager.Dispatch(EEvent.PLAYER_ATTR_UPDATE);
            }
        }
    }
}