using System.Collections.Generic;
using MemoryPack;

namespace GameLogic
{
    public enum eCatChestGameItemCategoryType
    {
        Cat,         // 猫
        Waste,      // 破烂
        CatFood,    // 食物
        Coin,       // 金币
    }

    [MemoryPackable]
    public partial class CatChestGameSaveVO
    {
        [MemoryPackOrder(0)]
        public List<string> BackpackItemIDs { get; set; }
        [MemoryPackOrder(1)]
        public int Level { get; set; }
        [MemoryPackOrder(2)]
        public int CoinAmount { get; set; }
    }

    [MemoryPackable]
    public partial class CatChestGameItemConfigVO
    {
        [MemoryPackOrder(0)]
        public string ID { get; set; }
        [MemoryPackOrder(1)]
        public eCatChestGameItemCategoryType CategoryType { get; set; }
        [MemoryPackOrder(2)]
        public string NextUpgradeID {get;set;}
        [MemoryPackOrder(3)]
        public int[] EffectValue { get; set; }
        [MemoryPackOrder(4)]
        public int SellCoinValue { get; set; }
    }
    

    public enum eCatChestGameRewardType
    {
        ITEM,
        MONEY,
    }

    public enum eCatChestGameItemExchangeType
    {
        Normal,  // 普通兑换
        Recycle, // 回收
        LimitedTime // 限时兑换
    }

    public enum eCatChestGameItemExchangeMatchType
    {
        All,
        Any,
    }

    [MemoryPackable]
    public partial class CatChestGameItemExchangeConfigVO
    {
        [MemoryPackOrder(0)]
        public string ID { get; set; }
        [MemoryPackOrder(1)]
        public string Name { get; set; }
        [MemoryPackOrder(2)]
        public eCatChestGameItemExchangeType ExchangeType { get; set; }
        [MemoryPackOrder(3)]
        public eCatChestGameItemExchangeMatchType ExchangeMatchType { get; set; }
        [MemoryPackOrder(4)]
        public string[] ExchangeItemIDs { get; set; }
        [MemoryPackOrder(5)]
        public int[] ExchangeItemCounts { get; set; }
        [MemoryPackOrder(6)]
        public string RewardItemsStr { get; set; }
        [MemoryPackOrder(7)]
        public int Level { get; set; }
    }

    [MemoryPackable]
    public partial class CatChestGameChestConfigVO
    {
        [MemoryPackOrder(0)]
        public string ID { get; set; }
        [MemoryPackOrder(1)]
        public int Level { get; set; }
        [MemoryPackOrder(2)]
        public string[] DropPoolIDs { get; set; }
        [MemoryPackOrder(3)]
        public int[] DropItemWeights { get; set; }
        [MemoryPackOrder(4)]
        public int OpenCost { get; set; }
    }

    [MemoryPackable]
    public partial class CatChestGameChestItemPoolConfigVO
    {
        [MemoryPackOrder(0)]
        public string ID { get; set; }
        [MemoryPackOrder(1)]
        public string[] ItemIDs { get; set; }
        [MemoryPackOrder(2)]
        public int[] Weights { get; set; }
    }
}