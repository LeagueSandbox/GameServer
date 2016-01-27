using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using static ENet.Native;

namespace IntWarsSharp.Core.Logic.PacketHandlers.Packets
{
    class HandleSynch : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            return false;
        }
    }
}
