namespace GameServerCore.Domain
{
    public interface IItemData
    {
        int ItemId { get; }
        string Name { get; }
        int MaxStack { get; }
        int Price { get; }
        string ItemGroup { get; }
        float SellBackModifier { get; }
        int RecipeItem1 { get; }
        int RecipeItem2 { get; }
        int RecipeItem3 { get; }
        int RecipeItem4 { get; }
        int TotalPrice { get; }

    }
}
