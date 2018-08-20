using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Logic.Domain
{
    public interface IItem
    {
        byte StackSize { get; }
        int TotalPrice { get; }
        IItemType ItemType { get; }
    }
}
