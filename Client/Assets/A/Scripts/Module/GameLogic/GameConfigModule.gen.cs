namespace GameLogic
{
    public partial class GameConfigModule
    {
        public void LoadConfig()
        {
            LoadConfigFromPath<CatChestGameChestConfigVO>("Config/CatChestGameChestConfigVO");
            LoadConfigFromPath<CatChestGameChestItemPoolConfigVO>("Config/CatChestGameChestItemPoolConfigVO");
            LoadConfigFromPath<CatChestGameItemConfigVO>("Config/CatChestGameItemConfigVO");
            LoadConfigFromPath<CatChestGameItemExchangeConfigVO>("Config/CatChestGameItemExchangeConfigVO");
            LoadConfigFromPath<FoodCellConfigVO>("Config/FoodCellConfigVO");
            LoadConfigFromPath<SymbolConfigVO>("Config/SymbolConfigVO");
        }
    }
}
