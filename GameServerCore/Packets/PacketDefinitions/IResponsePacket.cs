namespace GameServerCore.Packets.PacketDefinitions
{
    public interface IResponsePacket<T> : IPacket where T: ICoreResponse
    {
        // for responses
        byte[] Write(T msg);
    }
}
