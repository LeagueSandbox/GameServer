using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public abstract class PacketHandlerBase<TPacket> : IPacketHandler
        where TPacket : ClientPacketBase
    {
        public abstract PacketCmd PacketType { get; }
        public abstract Channel PacketChannel { get; }
        public abstract bool HandlePacketInternal(Peer peer, TPacket data);

        public bool HandlePacket<TPacket1>(Peer peer, TPacket1 data) where TPacket1 : ClientPacketBase
        {
            return HandlePacketInternal(peer, (TPacket)(object)data);
        }
    }
}
