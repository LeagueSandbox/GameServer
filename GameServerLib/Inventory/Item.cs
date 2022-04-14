using GameServerCore.Domain;
using LeagueSandbox.GameServer.GameObjects.Other;

namespace LeagueSandbox.GameServer.Inventory
{
    public class Item : Stackable, IItem
    {
        public int TotalPrice => ItemData.TotalPrice;
        public IItemData ItemData { get; }


        private Item(IItemData data)
        {
            ItemData = data;
            StackCount = 1;
        }

        public override bool IncrementStackCount()
        {
            if (StackCount >= ItemData.MaxStacks)
            {
                return false;
            }

            StackCount++;
            return true;
        }

        public override void SetStacks(int newStacks)
        {
            if (newStacks < 1 || newStacks > ItemData.MaxStacks)
                throw new System.Exception($"Cannot set stack size out of bounds (max is {ItemData.MaxStacks})");

            StackCount = newStacks;
        }

        public static Item CreateFromType(IItemData item)
        {
            return new Item(item);
        }
    }
}