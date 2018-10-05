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
            throw new System.NotImplementedException();
        }

        public static Item CreateFromType(IItemData item)
        {
            return new Item(item);
        }
    }
}