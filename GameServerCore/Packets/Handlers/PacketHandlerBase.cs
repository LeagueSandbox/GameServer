using GameServerCore.Packets.Enums;

namespace GameServerCore.Packets.Handlers
{
    public abstract class PacketHandlerBase : IPacketHandler
    {
        public abstract PacketCmd PacketType { get; }
        public abstract Channel PacketChannel { get; }
        public abstract bool HandlePacket(int userId, byte[] data);
    }
}
