using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO
{
    public struct PacketObject
    {
        public uint ObjectNetId { get; }

        public PacketObject(uint objectNetId)
        {
            ObjectNetId = objectNetId;
        }
    }
}
