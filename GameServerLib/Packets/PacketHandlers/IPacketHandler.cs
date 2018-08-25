using ENet;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public interface IPacketHandler
    {
        bool HandlePacket(Peer peer, byte[] data);
    }
}
