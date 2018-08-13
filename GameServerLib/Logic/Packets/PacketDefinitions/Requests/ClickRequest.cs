using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests
{
    public class ClickRequest
    {
        public uint TargetNetId { get; }

        public ClickRequest(uint targetNetId)
        {
            TargetNetId = targetNetId;
        }
    }
}
