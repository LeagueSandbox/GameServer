using GameServerCore.Packets.Enums;
using GameServerCore.Packets.PacketDefinitions;

namespace GameServerCore.Packets.Handlers
{
    public abstract class PacketHandlerBase<T> : IPacketHandler<T> where T: ICoreRequest
    {
        public abstract PacketCmd PacketType { get; }
        public abstract Channel PacketChannel { get; }
        public abstract bool HandlePacket(int userId, T req);
    }
}
