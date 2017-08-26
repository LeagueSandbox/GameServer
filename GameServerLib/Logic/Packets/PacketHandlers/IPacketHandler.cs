using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public interface IPacketHandler
    {
        PacketCmd PacketType { get; }
        Channel PacketChannel { get; }
        bool HandlePacket<TPacket>(Peer peer, TPacket data) where TPacket : ClientPacketBase;
    }
}
