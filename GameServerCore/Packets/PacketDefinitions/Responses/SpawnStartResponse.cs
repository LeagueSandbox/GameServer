namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class SpawnStartResponse : ICoreResponse
    {
        public int UserId { get; }
        public SpawnStartResponse(int userId)
        {
            UserId = userId;
        }
    }
}