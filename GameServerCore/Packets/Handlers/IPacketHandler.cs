using GameServerCore.Packets.PacketDefinitions;
using LeaguePackets;

namespace GameServerCore.Packets.Handlers
{
    public interface IPacketHandler<in T> where T: BasePacket
    {
        bool HandlePacket(int userId, T req);
    }
}
