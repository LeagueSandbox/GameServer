using ENet;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public interface IPacketHandler
    {
        bool HandlePacket(Peer peer, byte[] data);
    }
}
