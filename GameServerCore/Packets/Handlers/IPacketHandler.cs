using GameServerCore.Packets.PacketDefinitions;

namespace GameServerCore.Packets.Handlers
{
    public interface IPacketHandler<in T> where T: ICoreRequest
    {
        bool HandlePacket(int userId, T req);
    }
}
