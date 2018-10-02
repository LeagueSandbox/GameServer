using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Domain
{
    public interface IItemType: IStatsModifier
    {
        int ItemId { get; }
        string Name { get; }
        int MaxStack { get; }
        int Price { get; }
        string ItemGroup { get; }
        string SpellName { get; }
        float SellBackModifier { get; }
        int RecipeItem1 { get; }
        int RecipeItem2 { get; }
        int RecipeItem3 { get; }
        int RecipeItem4 { get; }
        IItemRecipe Recipe { get; }
        int TotalPrice { get; }
    }
}
