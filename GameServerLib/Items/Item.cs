using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.Items
{
    public class Item : IItem
    {
        public byte StackSize { get; private set; }
        public int TotalPrice => ItemType.TotalPrice;
        public ItemType ItemType { get; private set; }

        IItemType IItem.ItemType => ItemType;

        private Inventory _owner;

        private Item(Inventory owner, ItemType type)
        {
            _owner = owner;
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

        public static Item CreateFromType(Inventory inventory, ItemType item)
        {
            return new Item(inventory, item);
        }
    }
}