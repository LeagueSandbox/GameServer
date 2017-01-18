using ENetCS;

namespace LeagueSandbox.GameServer.Core.Logic
{
    public interface IPacketHandler
    {
        bool HandlePacket(Peer peer, byte[] data);
    }
}
