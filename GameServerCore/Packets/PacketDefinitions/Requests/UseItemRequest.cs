using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    class UseItemRequest
    {
        public int NetId { get; }
        public byte SlotId { get; }

        public UseItemRequest(int netId, byte slotId)
        {
            NetId = netId;
            SlotId = slotId;
        }
    }
}
