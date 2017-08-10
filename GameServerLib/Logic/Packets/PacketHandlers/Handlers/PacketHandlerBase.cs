using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public abstract class PacketHandlerBase : IPacketHandler
    {
        public abstract PacketCmd PacketType { get; }
        public abstract Channel PacketChannel { get; }
        public abstract bool HandlePacket(Peer peer, byte[] data);
    }
}
