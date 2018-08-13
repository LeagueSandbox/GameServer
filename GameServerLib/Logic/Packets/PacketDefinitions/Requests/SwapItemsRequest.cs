using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests
{
    public class SwapItemsRequest
    {
        public int NetId { get; }
        public byte SlotFrom { get; }
        public byte SlotTo { get; }

        public SwapItemsRequest(int netId, byte slotFrom, byte slotTo)
        {
            NetId = netId;
            SlotFrom = slotFrom;
            SlotTo = slotTo;
        }
    }
}
