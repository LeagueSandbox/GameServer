using ENet;
using GameServerCore.Packets.Enums;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public abstract class PacketHandlerBase : IPacketHandler
    {
        public abstract PacketCmd PacketType { get; }
        public abstract Channel PacketChannel { get; }
        public abstract bool HandlePacket(Peer peer, byte[] data);
    }
}
