using ENet;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleNull : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data)
        {
            return true;
        }
    }
}
