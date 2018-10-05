using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.Items
{
    public class Item : IItem
    {
        public int TotalPrice => ItemType.TotalPrice;
        public IItemType ItemType { get; }
        public byte StackCount { get; private set; }


        private Item(IItemType type)
        {
            ItemType = type;
            StackCount = 1;
        }
        
        public bool IncrementStackCount()
        {
            if (StackCount >= ItemType.MaxStack)
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
            throw new System.NotImplementedException();
        }

        public static Item CreateFromType(IItemType item)
        {
            return new Item(item);
        }
    }
}