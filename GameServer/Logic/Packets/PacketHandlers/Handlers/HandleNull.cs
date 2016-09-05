using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleNull : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data)
        {
            return true;
        }
    }
}
