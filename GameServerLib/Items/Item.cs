using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.Items
{
    public class Item : IItem
    {
        public byte StackSize { get; private set; }
        public int TotalPrice { get; private set; }
        public ItemData ItemData { get; private set; }

        IItemData IItem.ItemData => ItemData;

        private Inventory _owner;

        private Item(Inventory owner, ItemData data)
        {
            _owner = owner;
            ItemData = data;
            StackSize = 1;
        }

        public bool IncrementStackSize()
        {
            if (StackSize >= ItemData.MaxStack)
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

        public static Item CreateFromType(Inventory inventory, ItemData item)
        {
            return new Item(inventory, item);
        }
    }
}