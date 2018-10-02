using System.Collections.Generic;

namespace GameServerCore.Domain
{
    public interface IItemRecipe
    {
        int TotalPrice { get; }
        List<IItemType> GetItems();
    }
}