using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct AddUnitFOWArgs
    {
        public uint TargetNetId { get; }

        public AddUnitFOWArgs(uint targetNetId)
        {
            TargetNetId = targetNetId;
        }
    }
}
