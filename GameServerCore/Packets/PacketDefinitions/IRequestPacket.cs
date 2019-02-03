namespace GameServerCore.Packets.PacketDefinitions
{
    public interface IRequestPacket<out T> : IPacket where T: ICoreRequest
    {
        // for requests
        T Read(byte[] data);
    }
}
