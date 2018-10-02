using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.Items
{
    public class Item : IItem
    {
        public byte StackSize { get; private set; }
        public int TotalPrice => ItemType.TotalPrice;
        public IItemType ItemType { get; }

        private Item(IItemType type)
        {
            ItemType = type;
            StackSize = 1;
        }

        public bool IncrementStackSize()
        {
            if (StackSize >= ItemType.MaxStack)
            {
                return false;
            }

            StackSize++;
            return true;
        }

        public bool DecrementStackSize()
        {
            if (StackSize < 1)
            {
                return false;
            }

            StackSize--;
            return true;
        }

        public static Item CreateFromType(IItemType item)
        {
            return new Item(item);
        }
    }
}