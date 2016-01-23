using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet.Native;

namespace IntWarsSharp.Core.Logic.PacketHandlers.Packets
{
    class HandleLoadPing : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, ENetPacket* packet)
        {
            return false;
        }
    }
}
