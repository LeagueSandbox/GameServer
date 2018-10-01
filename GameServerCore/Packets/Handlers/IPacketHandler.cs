namespace GameServerCore.Packets.Handlers
{
    public interface IPacketHandler
    {
        bool HandlePacket(int userId, byte[] data);
    }
}
