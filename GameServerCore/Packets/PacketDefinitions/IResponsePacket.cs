namespace GameServerCore.Packets.PacketDefinitions
{
    public interface IResponsePacket<in T> : IPacket where T: ICoreResponse
    {
        // for responses
        byte[] Write(T msg);
    }
}
