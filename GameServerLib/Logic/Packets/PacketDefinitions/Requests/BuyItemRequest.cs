using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests
{
    public class BuyItemRequest
    {
        public int NetId { get; }
        public int ItemId { get; }

        public BuyItemRequest(int netId, int itemId)
        {
            NetId = netId;
            ItemId = itemId;
        }
    }
}
