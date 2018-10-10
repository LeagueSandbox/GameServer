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
        int[] RecipeItems { get; }
        int TotalPrice { get; }
    }
}
