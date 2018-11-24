namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class DebugPacketResponse : ICoreResponse
    {
        public int UserId { get; }
        public byte[] Data { get; }
        public DebugPacketResponse(int userId, byte[] data)
        {
            UserId = userId;
            Data = data;
        }
    }
}