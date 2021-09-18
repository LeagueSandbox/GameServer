using System.Dynamic;
using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Domain
{
    public interface IItemData : IStatsModifier
    {
        int ItemId { get; }
        string Name { get; }
        int MaxStacks { get; }
        int Price { get; }
        string ItemGroup { get; }
        bool Consumed { get; }
        string SpellName { get; }
        float SellBackModifier { get; }
        int[] RecipeItem { get; }
        IItemRecipe Recipe { get; }
        int TotalPrice { get; }
    }
}
