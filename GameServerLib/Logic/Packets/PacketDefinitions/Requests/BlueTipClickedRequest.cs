using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests
{
    public class BlueTipClickedRequest
    {
        public uint PlayerNetId { get; }
        public uint NetId { get; }

        public BlueTipClickedRequest(uint playerNetId, uint netId)
        {
            PlayerNetId = playerNetId;
            NetId = netId;
        }
    }
}
