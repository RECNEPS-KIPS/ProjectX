using MemoryPack;
using System.Collections.Generic;

namespace GameLogic
{
    #region FurstGame
    #region MemoryPackable  
    [MemoryPackable]
    public partial class FoodCellSaveVO
    {
        [MemoryPackOrder(0)]
        public int foodConfigID;
        [MemoryPackOrder(1)]
        public int foodCDSpeedLevel;
        [MemoryPackOrder(2)]
        public int foodMaxAmountLevel;
        [MemoryPackOrder(3)]
        public int curFoodAmount;
        [MemoryPackOrder(4)]
        public bool isUnlock;
    }

    [MemoryPackable]
    public partial class FoodCellConfigVO
    {
        [MemoryPackOrder(0)]
        public int FoodConfigID { get; set; }
        [MemoryPackOrder(1)]
        public string SpriteName { get; set; }
        [MemoryPackOrder(2)]
        public int MaxLevel { get; set; }
        [MemoryPackOrder(3)]
        public float PerLevelCDAddSpeed { get; set; }
        [MemoryPackOrder(4)]
        public float PerLevelFoodAmountAdd { get; set; }
        [MemoryPackOrder(5)]
        public int LevelUpCostCoin { get; set; }
        [MemoryPackOrder(6)]
        public int UnlockCoin { get; set; }

    }

    [MemoryPackable]
    public partial class PlayerSaveVO
    {
        [MemoryPackOrder(0)]
        public int coinAmount;
        [MemoryPackOrder(1)]
        public List<FoodCellSaveVO> foodCellSaves;
        [MemoryPackOrder(2)]
        public List<AnimalCustomerInfo> animalCustomerInfos;
    }
    #endregion

    public class AnimalCustomerInfo
    {
        public string spriteName;
        public int heartAmount;
        public int coinAmount;
        public List<RequestInfo> requestInfos;
    }

    public class RequestInfo
    {
        public int foodID;
        public int requestCount;
    }
    #endregion

    #region SlotMachineGame
    [MemoryPackable]
    public partial class SlotMachineGameSaveVO
    {
        [MemoryPackOrder(0)]
        public int coinAmount;
        [MemoryPackOrder(1)]
        public int spinCount;
        
        
    }

    [MemoryPackable]
    public partial class HeroSaveVO
    {
        [MemoryPackOrder(0)]
        public int curHealthValue;
        [MemoryPackOrder(1)]
        public int maxHealthValue;
        [MemoryPackOrder(2)]
        public int attackValue;
    }

    [MemoryPackable]
    public partial class SymbolConfigVO
    {
        [MemoryPackOrder(0)]
        public string SymbolID { get; set; }
        [MemoryPackOrder(1)]
        public eSymbolType SymbolType { get; set; }
        [MemoryPackOrder(2)]
        public int SymbolValue { get; set; }
        [MemoryPackOrder(3)]
        public string SpriteName { get; set; }
        [MemoryPackOrder(4)]
        public int Weight { get; set; }
        [MemoryPackOrder(5)]
        public int BuyCoin { get; set; }
    }

    public enum eSymbolType
    {
        None,
        AddHP,
        AddAttack,
        AddMaxHP,
        LuckCat,
        Coin,
        SpinCoin
    }
    #endregion
}