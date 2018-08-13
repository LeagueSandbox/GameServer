using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests
{
    public class UseObjectRequest
    {
        public uint TargetNetId { get; }

        public UseObjectRequest(uint targetNetId)
        {
            TargetNetId = targetNetId;
        }
    }
}
