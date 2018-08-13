using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests
{
    public class SellItemRequest
    {
        public int NetId { get; }
        public byte SlotId { get; }

        public SellItemRequest(int netId, byte slotId)
        {
            NetId = netId;
            SlotId = slotId;
        }
    }
}
