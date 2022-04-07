using GameServerCore.Packets.PacketDefinitions;
using LeaguePackets;

namespace GameServerCore.Packets.Handlers
{
    public abstract class PacketHandlerBase<T> : IPacketHandler<T> where T: BasePacket
    {
        public abstract bool HandlePacket(int userId, T req);
    }
}
