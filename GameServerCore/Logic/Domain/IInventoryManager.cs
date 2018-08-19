using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Logic.Domain
{
    public interface IInventoryManager
    {
        IItem GetItem(int slot);
        void RemoveItem(int slot);
        byte GetItemSlot(IItem item);
        IItem SetExtraItem(byte slot, IItemType item);
        void SwapItems(int slot1, int slot2);
    }
}
