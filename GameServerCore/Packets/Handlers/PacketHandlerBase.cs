using GameServerCore.Packets.PacketDefinitions;

namespace GameServerCore.Packets.Handlers
{
    public abstract class PacketHandlerBase<T> : IPacketHandler<T> where T: ICoreRequest
    {
        public abstract bool HandlePacket(int userId, T req);
    }
}
