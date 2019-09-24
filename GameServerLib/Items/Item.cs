using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.Items
{
    public class Item : IItem
    {
        public int TotalPrice => ItemData.TotalPrice;
        public IItemData ItemData { get; }
        public byte StackCount { get; private set; }


        private Item(IItemData data)
        {
            ItemData = data;
            StackCount = 1;
        }
        
        public bool IncrementStackCount()
        {
            if (StackCount >= ItemData.MaxStack)
            {
                return false;
            }

            StackCount++;
            return true;
        }

        public bool DecrementStackCount()
        {
            if (StackCount < 1)
            {
                return false;
            }

            StackCount--;
            return true;
        }

        public void SetStacks(byte newStacks)
        {
            if(newStacks < 1 || newStacks > ItemData.MaxStack)
                throw new System.Exception($"Cannot set stack size out of bounds (max is {ItemData.MaxStack})");

            StackCount = newStacks;
        }

        public static Item CreateFromType(IItemData item)
        {
            return new Item(item);
        }
    }
}