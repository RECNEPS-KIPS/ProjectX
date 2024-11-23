using System;

using Framework.Core.Manager.Config;
using Framework.Core.Manager.Event;

namespace GamePlay.Player
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
        
        public int CurDefense;
        public int CurAttack;
        public int CurHP;
        public int CurStamina;
        public int CurThirsty;
        public int CurHungry;
        
        public int GrowthTemp = -1;
        public int Level = 1;

        private int GrowthHP;
        private int GrowthStamina;
        private int GrowthThirsty;
        private int GrowthHungry;
        private int GrowthDefense;
        private int GrowthAttack;

        /// <summary>
        /// 初始化玩家属性
        /// </summary>
        public void InitAttrValues()
        {
            var characterCf = ConfigManager.GetConfigByID(EConfig.Character, PlayerManager.PROTAGONIST_ID);
            if (characterCf == null) return;
            BaseDefense = characterCf["baseDefense"];
            BaseAttack = characterCf["baseAttack"];
            BaseHP = characterCf["baseHP"];
            BaseStamina = characterCf["baseStamina"];
            BaseThirsty = characterCf["baseThirsty"];
            BaseHungry = characterCf["baseHungry"];
            GrowthTemp = characterCf["growthTemp"];
            if (GrowthTemp < 0) return;
            var growthCf = ConfigManager.GetConfigByID(EConfig.GrowthTemp, GrowthTemp);

            if (growthCf == null) return;
            GrowthHP = growthCf["hp"];
            GrowthStamina = growthCf["stamina"];
            GrowthThirsty = growthCf["thirsty"];
            GrowthHungry = growthCf["hungry"];
            GrowthDefense = growthCf["defense"];
            GrowthAttack = growthCf["attack"];
                        
            // Level = //todo:读取存档
            //todo:读取存档
                        
            CurDefense = CurMaxDefense;
            CurAttack = CurMaxAttack;
            CurHP = CurMaxHP;
            CurStamina = CurMaxStamina;
            CurThirsty = CurMaxThirsty;
            CurHungry = CurMaxHungry;
                        
            EventManager.Dispatch(EEvent.PLAYER_ATTR_UPDATE);
            LogManager.Log("PlayerAttr",ToString());
        }
        public int CurMaxDefense => BaseDefense + (Level - 1) * GrowthDefense;
        public int CurMaxAttack => BaseAttack + (Level - 1) * GrowthAttack;
        public int CurMaxHP => BaseHP + (Level - 1) * GrowthHP;
        public int CurMaxStamina => BaseStamina + (Level - 1) * GrowthStamina;
        public int CurMaxThirsty => BaseThirsty + (Level - 1) * GrowthThirsty;
        public int CurMaxHungry => BaseHungry + (Level - 1) * GrowthHungry;

        public override string ToString()
        {
            return $"BaseDefense:{ BaseDefense}, BaseAttack:{ BaseAttack}, BaseHP:{ BaseHP}, BaseStamina:{ BaseStamina}, BaseThirsty:{ BaseThirsty}, BaseHungry:{ BaseHungry}, CurDefense:{ CurDefense}, CurAttack:{ CurAttack}, CurHP:{ CurHP}, CurStamina:{ CurStamina}, CurThirsty:{ CurThirsty}, CurHungry:{ CurHungry}, GrowthTemp:{ GrowthTemp}, Level:{ Level}, GrowthHP:{ GrowthHP}, GrowthStamina:{ GrowthStamina}, GrowthThirsty:{ GrowthThirsty}, GrowthHungry:{ GrowthHungry}, GrowthDefense:{ GrowthDefense}, GrowthAttack:{ GrowthAttack},";
        }
    }
}