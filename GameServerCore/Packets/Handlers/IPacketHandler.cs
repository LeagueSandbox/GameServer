using GameServerCore.Packets.PacketDefinitions;

namespace GameServerCore.Packets.Handlers
{
    public interface IPacketHandler<T>
    {
        bool HandlePacket(int userId, T req);
    }
}
